using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Hammer.Auction.Tests.Api;

/// <summary>
/// Integration tests for <see cref="Hammer.Auction.Api.Controllers.NewsController"/>.
/// </summary>
public sealed class NewsControllerTests : IDisposable
{
    private readonly INewsClient _newsClient = Substitute.For<INewsClient>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public NewsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test");
            builder.UseSetting("Kafka:BootstrapServers", "localhost:9092");
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? kafkaWorker = services.SingleOrDefault(
                    d => d.ImplementationType?.Name == "KafkaConsumerWorker");

                if (kafkaWorker is not null)
                    services.Remove(kafkaWorker);

                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AuctionDbContext>) ||
                        (d.ServiceType.IsGenericType &&
                         d.ServiceType.GetGenericTypeDefinition().FullName?.StartsWith(
                             "Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true))
                    .ToList();

                foreach (ServiceDescriptor descriptor in toRemove)
                    services.Remove(descriptor);

                services.AddDbContext<AuctionDbContext>(options =>
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

                services.AddScoped(_ => _newsClient);
            });
        });

        _client = _factory.CreateClient();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetRecent_ShouldReturn200WithListAsync()
    {
        _newsClient.GetRecentAsync(5, Arg.Any<CancellationToken>()).Returns([CreateNews()]);

        HttpResponseMessage response = await _client.GetAsync("/news/recent?count=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<NewsResponse>? body = await response.Content.ReadFromJsonAsync<List<NewsResponse>>();
        body.Should().NotBeNull();
        body!.Should().ContainSingle();
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturn200Async()
    {
        var id = Guid.NewGuid();
        _newsClient.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(CreateNews());

        HttpResponseMessage response = await _client.GetAsync($"/news/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenMissing_ShouldReturn404Async()
    {
        _newsClient.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((NewsResponse?)null);

        HttpResponseMessage response = await _client.GetAsync($"/news/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetList_ShouldReturn200WithPagedResponseAsync()
    {
        _newsClient.GetPagedAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<NewsResponse>([CreateNews()], 1, 20, 1, 1));

        HttpResponseMessage response = await _client.GetAsync("/news?page=1&size=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResponse<NewsResponse>? body = await response.Content.ReadFromJsonAsync<PagedResponse<NewsResponse>>();
        body.Should().NotBeNull();
        body!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Search_WithKeyword_ShouldReturn200Async()
    {
        _newsClient.SearchByTitleAsync("경매", 1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<NewsResponse>([CreateNews()], 1, 20, 1, 1));

        HttpResponseMessage response = await _client.GetAsync("/news/search?keyword=경매");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Search_WithEmptyKeyword_ShouldReturn400Async()
    {
        HttpResponseMessage response = await _client.GetAsync("/news/search?keyword=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static NewsResponse CreateNews() =>
        new(Guid.NewGuid(), "경매", "제목", "https://example.com/1", "https://example.com/1", "요약", DateTimeOffset.UtcNow);
}
