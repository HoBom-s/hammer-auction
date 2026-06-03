using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetNewsById;

/// <summary>
/// 뉴스 상세 조회 유스케이스.
/// </summary>
public interface IGetNewsByIdUseCase
{
    /// <summary>
    /// 식별자로 뉴스 상세를 조회한다.
    /// </summary>
    /// <param name="id">기사 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>뉴스 상세.</returns>
    public Task<NewsResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
}
