using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;

namespace Hammer.Auction.Application.UseCases.GetRecentNews;

/// <summary>
/// hammer-support에서 최신 뉴스 목록을 조회한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetRecentNewsUseCase(INewsClient newsClient) : IGetRecentNewsUseCase
{
    private const int MaxCount = 20;

    /// <inheritdoc />
    public async Task<IReadOnlyList<NewsResponse>> ExecuteAsync(int count, CancellationToken ct = default)
    {
        if (count is < 1 or > MaxCount)
            throw new BadRequestException($"Count must be between 1 and {MaxCount}, but was {count}.");

        return await newsClient.GetRecentAsync(count, ct);
    }
}
