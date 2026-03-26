using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Infrastructure.Kafka;
using Hammer.Auction.Infrastructure.Persistence;
using Hammer.Auction.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.Auction.Infrastructure;

/// <summary>
/// Registers infrastructure layer services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the DI container.
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<AuctionDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IKamcoAuctionItemRepository, KamcoAuctionItemRepository>();
        services.AddScoped<IInstitutionAuctionItemRepository, InstitutionAuctionItemRepository>();
        services.AddScoped<IOnbidCodeInfoRepository, OnbidCodeInfoRepository>();

        if (!string.IsNullOrWhiteSpace(configuration["Kafka:BootstrapServers"]))
        {
            services.AddSingleton<IKafkaMessageHandler, KamcoAuctionItemHandler>();
            services.AddSingleton<IKafkaMessageHandler, InstitutionAuctionItemHandler>();
            services.AddSingleton<IKafkaMessageHandler, OnbidCodeInfoHandler>();
            services.AddHostedService<KafkaConsumerWorker>();
        }

        services
            .AddHealthChecks()
            .AddDbContextCheck<AuctionDbContext>();

        return services;
    }
}
