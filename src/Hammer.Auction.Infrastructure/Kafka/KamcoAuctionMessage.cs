namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
/// DTO for deserializing KAMCO auction items from the Kafka topic.
/// </summary>
internal sealed record KamcoAuctionMessage(
    long PlnmNo,
    long PbctNo,
    long CltrNo,
    string CltrNm,
    string CtgrFullNm,
    string LdnmAdrs,
    string NmrdAdrs,
    long MinBidPrc,
    long ApslAsesAvgAmt,
    string BidMtdNm,
    string PbctCltrStatNm,
    string PbctBegnDtm,
    string PbctClsDtm,
    int UscbdCnt,
    int IqryCnt,
    IReadOnlyList<string>? CltrImgFiles);
