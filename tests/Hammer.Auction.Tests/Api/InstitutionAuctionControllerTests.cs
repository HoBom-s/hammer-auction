using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;
using Hammer.Auction.Domain.ValueObjects;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hammer.Auction.Tests.Api;

/// <summary>
/// Integration tests for <see cref="Hammer.Auction.Api.Controllers.InstitutionAuctionController"/>.
/// </summary>
public sealed class InstitutionAuctionControllerTests : IDisposable
{
    private readonly IGetInstitutionAuctionItemsUseCase _getItems =
        Substitute.For<IGetInstitutionAuctionItemsUseCase>();

    private readonly IGetInstitutionAuctionItemByIdUseCase _getItemById =
        Substitute.For<IGetInstitutionAuctionItemByIdUseCase>();

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public InstitutionAuctionControllerTests()
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

                services.AddScoped(_ => _getItems);
                services.AddScoped(_ => _getItemById);
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
        var items = new List<InstitutionAuctionItemResponse> { CreateResponse(1), CreateResponse(2) };
        PagedResponse<InstitutionAuctionItemResponse> paged = new(items, 1, 20, 2, 1);

        _getItems.ExecuteAsync(Arg.Any<GetInstitutionAuctionItemsRequest>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        HttpResponseMessage response = await _client.GetAsync("/institution-auctions/items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PagedResponse<InstitutionAuctionItemResponse>? body =
            await response.Content.ReadFromJsonAsync<PagedResponse<InstitutionAuctionItemResponse>>();

        body.Should().NotBeNull();
        body!.Items.Should().HaveCount(2);
        body.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetItems_WithQueryParameters_ShouldPassToUseCaseAsync()
    {
        PagedResponse<InstitutionAuctionItemResponse> empty = new([], 2, 5, 0, 0);

        _getItems.ExecuteAsync(Arg.Any<GetInstitutionAuctionItemsRequest>(), Arg.Any<CancellationToken>())
            .Returns(empty);

        HttpResponseMessage response = await _client.GetAsync(
            "/institution-auctions/items?page=2&size=5&org=서울&category=토지&keyword=공매");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await _getItems.Received(1).ExecuteAsync(
            Arg.Is<GetInstitutionAuctionItemsRequest>(r =>
                r.Page == 2 && r.Size == 5 && r.Org == "서울" && r.Category == "토지" && r.Keyword == "공매"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetItemById_WithExistingItem_ShouldReturn200Async()
    {
        InstitutionAuctionItemResponse item = CreateResponse(1);
        _getItemById.ExecuteAsync(new InstitutionAuctionItemId(1), Arg.Any<CancellationToken>()).Returns(item);

        HttpResponseMessage response = await _client.GetAsync("/institution-auctions/items/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        InstitutionAuctionItemResponse? body =
            await response.Content.ReadFromJsonAsync<InstitutionAuctionItemResponse>();

        body.Should().NotBeNull();
        body!.PlnmNm.Should().Be("Test Item");
    }

    [Fact]
    public async Task GetItemById_WithNonExistentItem_ShouldReturn404Async()
    {
        _getItemById.ExecuteAsync(new InstitutionAuctionItemId(999), Arg.Any<CancellationToken>())
            .ThrowsAsync(new NotFoundException("InstitutionAuctionItem with ID 999 was not found"));

        HttpResponseMessage response = await _client.GetAsync("/institution-auctions/items/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static InstitutionAuctionItemResponse CreateResponse(long id)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new InstitutionAuctionItemResponse(
            id,
            100 + id,
            200 + id,
            "01",
            "공매공고",
            "02",
            "전자입찰",
            "Test Item",
            "서울특별시",
            "20260325",
            $"ORG-{id}",
            $"MNG-{id}",
            "03",
            "일반경쟁",
            "01",
            "총액",
            "01",
            "매각",
            "01",
            "토지",
            now,
            now.AddDays(2),
            now.AddDays(3),
            "CTG001",
            "토지 / 대지",
            now,
            now);
    }
}
