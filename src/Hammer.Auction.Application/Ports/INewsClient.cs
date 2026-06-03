using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.Ports;

/// <summary>
/// hammer-support 서비스에서 수집된 뉴스를 조회한다.
/// </summary>
public interface INewsClient
{
    /// <summary>
    /// 최신 뉴스 목록을 조회한다.
    /// </summary>
    /// <param name="count">조회할 기사 수.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>최신 뉴스 목록. 실패 시 빈 목록.</returns>
    public Task<IReadOnlyList<NewsResponse>> GetRecentAsync(int count, CancellationToken ct = default);

    /// <summary>
    /// 식별자로 뉴스 상세를 조회한다.
    /// </summary>
    /// <param name="id">기사 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>해당 기사, 없거나 실패 시 <c>null</c>.</returns>
    public Task<NewsResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 뉴스를 페이지 단위로 조회한다 (최신순).
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작).</param>
    /// <param name="size">페이지 크기.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>페이지 결과. 실패 시 빈 페이지.</returns>
    public Task<PagedResponse<NewsResponse>> GetPagedAsync(int page, int size, CancellationToken ct = default);

    /// <summary>
    /// 제목에 키워드가 포함된 뉴스를 페이지 단위로 검색한다 (최신순).
    /// </summary>
    /// <param name="keyword">제목 검색 키워드.</param>
    /// <param name="page">페이지 번호 (1부터 시작).</param>
    /// <param name="size">페이지 크기.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>검색 페이지 결과. 실패 시 빈 페이지.</returns>
    public Task<PagedResponse<NewsResponse>> SearchByTitleAsync(string keyword, int page, int size, CancellationToken ct = default);
}
