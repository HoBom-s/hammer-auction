namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
/// DTO for deserializing real estate trade items from the Kafka topic.
/// </summary>
internal sealed record RealEstateTradeMessage(
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
    int? BuildYear);
