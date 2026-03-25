using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Hammer.Auction.Tests.Api;

/// <summary>
/// Integration tests for <see cref="Hammer.Auction.Api.Controllers.AuctionController"/>.
/// </summary>
public sealed class AuctionControllerTests : IDisposable
{
    private readonly IKamcoAuctionItemRepository _repository = Substitute.For<IKamcoAuctionItemRepository>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuctionControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test");
            builder.UseSetting("Kafka:BootstrapServers", "localhost:9092");
            builder.ConfigureServices(services =>
            {
                // Remove Kafka hosted service
                ServiceDescriptor? kafkaWorker = services.SingleOrDefault(
                    d => d.ImplementationType?.Name == "KafkaConsumerWorker");

                if (kafkaWorker is not null)
                    services.Remove(kafkaWorker);

                // Remove all EF Core / Npgsql registrations to replace with InMemory
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

                services.AddScoped(_ => _repository);
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
    public async Task GetItems_ShouldReturn200WithPagedResponseAsync()
    {
        List<KamcoAuctionItem> items = [CreateEntity(1, "Item A"), CreateEntity(2, "Item B")];
        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 2));

        HttpResponseMessage response = await _client.GetAsync("/hammer-auctions/items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PagedResponse<KamcoAuctionItemResponse>? body =
            await response.Content.ReadFromJsonAsync<PagedResponse<KamcoAuctionItemResponse>>();

        body.Should().NotBeNull();
        body!.Items.Should().HaveCount(2);
        body.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetItems_WithQueryParameters_ShouldPassToUseCaseAsync()
    {
        _repository.GetPagedAsync(2, 5, "진행", "토지", "서울", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<KamcoAuctionItem>(), 0));

        HttpResponseMessage response = await _client.GetAsync(
            "/hammer-auctions/items?page=2&size=5&status=진행&category=토지&keyword=서울");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await _repository.Received(1).GetPagedAsync(
            2,
            5,
            "진행",
            "토지",
            "서울",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetItemById_WithExistingItem_ShouldReturn200Async()
    {
        KamcoAuctionItem entity = CreateEntity(1, "Test Item");
        typeof(KamcoAuctionItem).GetProperty(nameof(KamcoAuctionItem.Id))!.SetValue(entity, 1L);
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

        HttpResponseMessage response = await _client.GetAsync("/hammer-auctions/items/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        KamcoAuctionItemResponse? body =
            await response.Content.ReadFromJsonAsync<KamcoAuctionItemResponse>();

        body.Should().NotBeNull();
        body!.CltrNm.Should().Be("Test Item");
    }

    [Fact]
    public async Task GetItemById_WithNonExistentItem_ShouldReturn404Async()
    {
        _repository.GetByIdAsync(999L, Arg.Any<CancellationToken>())
            .Returns((KamcoAuctionItem?)null);

        HttpResponseMessage response = await _client.GetAsync("/hammer-auctions/items/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static KamcoAuctionItem CreateEntity(long plnmNo, string cltrNm)
    {
        return KamcoAuctionItem.Create(
            plnmNo,
            1,
            1,
            cltrNm,
            "Category",
            "Addr1",
            "Addr2",
            100,
            200,
            "Method",
            "Status",
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);
    }
}
