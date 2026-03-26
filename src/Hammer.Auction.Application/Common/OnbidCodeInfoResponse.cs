using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     Response DTO for an Onbid code info entry.
/// </summary>
/// <param name="Id">고유 식별자.</param>
/// <param name="CtgrId">코드 ID.</param>
/// <param name="CtgrNm">코드명.</param>
/// <param name="CtgrHirkId">상위 코드 ID.</param>
/// <param name="CtgrHirkNm">상위 코드명.</param>
/// <param name="CreatedAt">최초 수집일시 (UTC).</param>
/// <param name="UpdatedAt">최종 갱신일시 (UTC).</param>
public sealed record OnbidCodeInfoResponse(
    long Id,
    string CtgrId,
    string CtgrNm,
    string CtgrHirkId,
    string CtgrHirkNm,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    /// <summary>
    ///     Maps a domain entity to a response DTO.
    /// </summary>
    public static OnbidCodeInfoResponse FromEntity(OnbidCodeInfo entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new OnbidCodeInfoResponse(
            entity.Id,
            entity.CtgrId,
            entity.CtgrNm,
            entity.CtgrHirkId,
            entity.CtgrHirkNm,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
