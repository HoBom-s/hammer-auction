using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;

namespace Hammer.Auction.Application.UseCases.GetNewsList;

/// <summary>
/// hammer-support에서 뉴스를 페이지 단위로 조회한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetNewsListUseCase(INewsClient newsClient) : IGetNewsListUseCase
{
    private const int MaxSize = 100;

    /// <inheritdoc />
    public async Task<PagedResponse<NewsResponse>> ExecuteAsync(int page, int size, CancellationToken ct = default)
    {
        if (page < 1)
            throw new BadRequestException($"Page must be 1 or greater, but was {page}.");

        if (size is < 1 or > MaxSize)
            throw new BadRequestException($"Size must be between 1 and {MaxSize}, but was {size}.");

        return await newsClient.GetPagedAsync(page, size, ct);
    }
}
