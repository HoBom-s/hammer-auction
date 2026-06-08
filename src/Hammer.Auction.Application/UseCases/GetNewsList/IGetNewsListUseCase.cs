using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetNewsList;

/// <summary>
/// 뉴스 페이지네이션 조회 유스케이스.
/// </summary>
public interface IGetNewsListUseCase
{
    /// <summary>
    /// 뉴스를 페이지 단위로 조회한다 (최신순).
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작).</param>
    /// <param name="size">페이지 크기.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>페이지 결과.</returns>
    public Task<PagedResponse<NewsResponse>> ExecuteAsync(int page, int size, CancellationToken ct = default);
}
