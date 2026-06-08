using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.GetCodeInfoById;
using Hammer.Auction.Application.UseCases.GetCodeInfos;
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
/// Integration tests for <see cref="Hammer.Auction.Api.Controllers.CodeInfoController"/>.
/// </summary>
public sealed class CodeInfoControllerTests : IDisposable
{
    private readonly IGetCodeInfosUseCase _getCodeInfos = Substitute.For<IGetCodeInfosUseCase>();
    private readonly IGetCodeInfoByIdUseCase _getCodeInfoById = Substitute.For<IGetCodeInfoByIdUseCase>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CodeInfoControllerTests()
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

                services.AddScoped(_ => _getCodeInfos);
                services.AddScoped(_ => _getCodeInfoById);
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
    public async Task GetCodeInfos_ShouldReturn200WithPagedResponseAsync()
    {
        var items = new List<OnbidCodeInfoResponse> { CreateResponse(1), CreateResponse(2) };
        PagedResponse<OnbidCodeInfoResponse> paged = new(items, 1, 100, 2, 1);

        _getCodeInfos.ExecuteAsync(Arg.Any<GetCodeInfosRequest>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        HttpResponseMessage response = await _client.GetAsync("/code-infos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PagedResponse<OnbidCodeInfoResponse>? body =
            await response.Content.ReadFromJsonAsync<PagedResponse<OnbidCodeInfoResponse>>();

        body.Should().NotBeNull();
        body!.Items.Should().HaveCount(2);
        body.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCodeInfos_WithQueryParameters_ShouldPassToUseCaseAsync()
    {
        PagedResponse<OnbidCodeInfoResponse> empty = new([], 1, 100, 0, 0);

        _getCodeInfos.ExecuteAsync(Arg.Any<GetCodeInfosRequest>(), Arg.Any<CancellationToken>())
            .Returns(empty);

        HttpResponseMessage response = await _client.GetAsync("/code-infos?page=1&size=50&parentId=ROOT");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await _getCodeInfos.Received(1).ExecuteAsync(
            Arg.Is<GetCodeInfosRequest>(r => r.Page == 1 && r.Size == 50 && r.ParentId == "ROOT"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCodeInfoById_WithExistingItem_ShouldReturn200Async()
    {
        OnbidCodeInfoResponse item = CreateResponse(1);
        _getCodeInfoById.ExecuteAsync(new OnbidCodeInfoId(1), Arg.Any<CancellationToken>()).Returns(item);

        HttpResponseMessage response = await _client.GetAsync("/code-infos/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        OnbidCodeInfoResponse? body =
            await response.Content.ReadFromJsonAsync<OnbidCodeInfoResponse>();

        body.Should().NotBeNull();
        body!.CtgrNm.Should().Be("토지");
    }

    [Fact]
    public async Task GetCodeInfoById_WithNonExistentItem_ShouldReturn404Async()
    {
        _getCodeInfoById.ExecuteAsync(new OnbidCodeInfoId(999), Arg.Any<CancellationToken>())
            .ThrowsAsync(new NotFoundException("OnbidCodeInfo with ID 999 was not found"));

        HttpResponseMessage response = await _client.GetAsync("/code-infos/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static OnbidCodeInfoResponse CreateResponse(long id)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new OnbidCodeInfoResponse(
            id,
            $"CTG{id:D3}",
            "토지",
            "ROOT",
            "전체",
            now,
            now);
    }
}
