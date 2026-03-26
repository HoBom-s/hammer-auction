namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
/// DTO for deserializing institution auction items from the Kafka topic.
/// </summary>
internal sealed record InstitutionAuctionMessage(
    long PlnmNo,
    long PbctNo,
    string PlnmKindCd,
    string PlnmKindNm,
    string BidDvsnCd,
    string BidDvsnNm,
    string PlnmNm,
    string OrgNm,
    string PlnmDt,
    string OrgPlnmNo,
    string PlnmMnmtNo,
    string BidMtdCd,
    string BidMtdNm,
    string TotAmtUnpcDvsnCd,
    string TotAmtUnpcDvsnNm,
    string DpslMtdCd,
    string DpslMtdNm,
    string PrptDvsnCd,
    string PrptDvsnNm,
    string PbctBegnDtm,
    string PbctClsDtm,
    string PbctExctDtm,
    string CtgrId,
    string CtgrFullNm);
