using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.SearchNews;

/// <summary>
/// 제목 기반 뉴스 검색 유스케이스.
/// </summary>
public interface ISearchNewsUseCase
{
    /// <summary>
    /// 제목에 키워드가 포함된 뉴스를 페이지 단위로 검색한다 (최신순).
    /// </summary>
    /// <param name="keyword">제목 검색 키워드.</param>
    /// <param name="page">페이지 번호 (1부터 시작).</param>
    /// <param name="size">페이지 크기.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>검색 페이지 결과.</returns>
    public Task<PagedResponse<NewsResponse>> ExecuteAsync(string keyword, int page, int size, CancellationToken ct = default);
}
