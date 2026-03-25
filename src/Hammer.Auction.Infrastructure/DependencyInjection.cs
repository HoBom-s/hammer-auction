using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Infrastructure.Kafka;
using Hammer.Auction.Infrastructure.Persistence;
using Hammer.Auction.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuctionDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IKamcoAuctionItemRepository, KamcoAuctionItemRepository>();

        services.AddSingleton<IKafkaMessageHandler, KamcoAuctionItemHandler>();
        services.AddHostedService<KafkaConsumerWorker>();

        services
            .AddHealthChecks()
            .AddDbContextCheck<AuctionDbContext>();

        return services;
    }
}
