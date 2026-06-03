using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;

namespace Hammer.Auction.Application.UseCases.GetNewsById;

/// <summary>
/// hammer-support에서 뉴스 상세를 조회한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetNewsByIdUseCase(INewsClient newsClient) : IGetNewsByIdUseCase
{
    /// <inheritdoc />
    public async Task<NewsResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        NewsResponse? news = await newsClient.GetByIdAsync(id, ct);

        return news ?? throw new NotFoundException($"News {id} not found.");
    }
}
