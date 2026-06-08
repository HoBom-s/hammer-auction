using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;

namespace Hammer.Auction.Application.UseCases.SearchNews;

/// <summary>
/// hammer-support에서 제목 기반으로 뉴스를 검색한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class SearchNewsUseCase(INewsClient newsClient) : ISearchNewsUseCase
{
    private const int MaxSize = 100;

    /// <inheritdoc />
    public async Task<PagedResponse<NewsResponse>> ExecuteAsync(string keyword, int page, int size, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new BadRequestException("Keyword is required.");

        if (page < 1)
            throw new BadRequestException($"Page must be 1 or greater, but was {page}.");

        if (size is < 1 or > MaxSize)
            throw new BadRequestException($"Size must be between 1 and {MaxSize}, but was {size}.");

        return await newsClient.SearchByTitleAsync(keyword, page, size, ct);
    }
}
