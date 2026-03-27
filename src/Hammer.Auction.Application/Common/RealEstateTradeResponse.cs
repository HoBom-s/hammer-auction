using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     Response DTO for a real estate trade record.
/// </summary>
/// <param name="Id">고유 식별자.</param>
/// <param name="LawdCd">법정동 시군구 코드 (5자리).</param>
/// <param name="PropertyType">부동산 유형 (0=Unknown, 1=Apartment, 2=Detached, 3=RowHouse, 4=Officetel, 5=Land, 6=Commercial).</param>
/// <param name="BuildingName">단지명.</param>
/// <param name="Jibun">지번.</param>
/// <param name="UmdNm">법정동명.</param>
/// <param name="DealAmount">거래금액 (만원).</param>
/// <param name="DealYear">계약년도.</param>
/// <param name="DealMonth">계약월.</param>
/// <param name="DealDay">계약일.</param>
/// <param name="Area">면적 (㎡).</param>
/// <param name="Floor">층.</param>
/// <param name="BuildYear">건축년도.</param>
/// <param name="CreatedAt">최초 수집일시 (UTC).</param>
/// <param name="UpdatedAt">최종 갱신일시 (UTC).</param>
public sealed record RealEstateTradeResponse(
    long Id,
    string LawdCd,
    int PropertyType,
    string? BuildingName,
    string Jibun,
    string UmdNm,
    long DealAmount,
    int DealYear,
    int DealMonth,
    int DealDay,
    decimal Area,
    int? Floor,
    int? BuildYear,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    /// <summary>
    ///     Maps a domain entity to a response DTO.
    /// </summary>
    public static RealEstateTradeResponse FromEntity(RealEstateTrade entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new RealEstateTradeResponse(
            entity.Id,
            entity.LawdCd,
            entity.PropertyType,
            entity.BuildingName,
            entity.Jibun,
            entity.UmdNm,
            entity.DealAmount,
            entity.DealYear,
            entity.DealMonth,
            entity.DealDay,
            entity.Area,
            entity.Floor,
            entity.BuildYear,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
