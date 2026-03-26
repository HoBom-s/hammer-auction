namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
/// DTO for deserializing code information items from the Kafka topic.
/// </summary>
internal sealed record OnbidCodeInfoMessage(
    string CtgrId,
    string CtgrNm,
    string CtgrHirkId,
    string CtgrHirkNm);
