using System.Diagnostics.CodeAnalysis;
using Confluent.Kafka;
using Hammer.Auction.Application;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Infrastructure.Cleanup;
using Hammer.Auction.Infrastructure.Http;
using Hammer.Auction.Infrastructure.Kafka;
using Hammer.Auction.Infrastructure.Outbox;
using Hammer.Auction.Infrastructure.Persistence;
using Hammer.Auction.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.Auction.Infrastructure;

/// <summary>
///     Registers infrastructure layer services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    /// <summary>
    ///     Adds infrastructure services to the DI container.
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
        services.AddScoped<IRealEstateTradeRepository, RealEstateTradeRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationSettingRepository, NotificationSettingRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<ISearchLogRepository, SearchLogRepository>();

        if (!string.IsNullOrWhiteSpace(configuration["Kafka:BootstrapServers"]))
        {
            services.AddSingleton<IKafkaMessageHandler, KamcoAuctionItemHandler>();
            services.AddSingleton<IKafkaMessageHandler, InstitutionAuctionItemHandler>();
            services.AddSingleton<IKafkaMessageHandler, OnbidCodeInfoHandler>();
            services.AddSingleton<IKafkaMessageHandler, RealEstateTradeHandler>();
            services.AddHostedService<KafkaConsumerWorker>();

            var bootstrapServers = configuration["Kafka:BootstrapServers"]!;

            services.AddSingleton<IProducer<string, string>>(_ =>
                new ProducerBuilder<string, string>(
                    new ProducerConfig { BootstrapServers = bootstrapServers }).Build());

            services.AddHostedService<OutboxPublisherWorker>();
        }

        services.Configure<UserApiSettings>(configuration.GetSection("UserApi"));

        services.AddHttpClient<IDeviceTokenClient, DeviceTokenClient>(client =>
        {
            var baseUri = configuration["UserApi:BaseUri"];

            if (!string.IsNullOrWhiteSpace(baseUri))
                client.BaseAddress = new Uri(baseUri);
        });

        services.Configure<OutboxSettings>(configuration.GetSection("Outbox"));
        services.Configure<CleanupSettings>(configuration.GetSection("Cleanup"));
        services.AddHostedService<DataCleanupWorker>();

        services
            .AddHealthChecks()
            .AddDbContextCheck<AuctionDbContext>();

        return services;
    }
}
