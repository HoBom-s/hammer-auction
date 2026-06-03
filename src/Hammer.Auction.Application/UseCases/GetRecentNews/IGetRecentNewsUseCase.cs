using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetRecentNews;

/// <summary>
/// 최신 뉴스 목록 조회 유스케이스.
/// </summary>
public interface IGetRecentNewsUseCase
{
    /// <summary>
    /// 최신 뉴스 목록을 조회한다.
    /// </summary>
    /// <param name="count">조회할 기사 수.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>최신 뉴스 목록.</returns>
    public Task<IReadOnlyList<NewsResponse>> ExecuteAsync(int count, CancellationToken ct = default);
}
