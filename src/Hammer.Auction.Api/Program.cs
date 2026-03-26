using System.IO.Compression;
using dotenv.net;
using Hammer.Auction.Api.Middleware;
using Hammer.Auction.Application;
using Hammer.Auction.Infrastructure;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "live";
var envFileName = $".env.{env}";
var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

while (dir is not null && !File.Exists(Path.Combine(dir.FullName, envFileName)))
    dir = dir.Parent;

if (dir is not null)
    DotEnv.Load(new DotEnvOptions(envFilePaths: [Path.Combine(dir.FullName, envFileName)]));
else
    await Console.Error.WriteLineAsync($"Warning: {envFileName} not found in any parent directory.");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(configuration =>
    configuration.ReadFrom.Configuration(builder.Configuration));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString, builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

WebApplication app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using IServiceScope scope = app.Services.CreateScope();
    AuctionDbContext db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseResponseCompression();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "0";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Cache-Control"] = "no-store";

    var path = context.Request.Path.Value ?? string.Empty;
    var isScalar = path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);

    context.Response.Headers["Content-Security-Policy"] = isScalar
        ? "default-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net"
        : "default-src 'none'";

    await next();
});

app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

#pragma warning disable CA1050, S1118
/// <summary>
/// Entry point marker for WebApplicationFactory in tests.
/// </summary>
internal partial class Program;
#pragma warning restore CA1050, S1118
