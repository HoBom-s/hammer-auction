namespace Hammer.Auction.Application.UseCases.GetCodeInfos;

/// <summary>
/// Request parameters for paginated code info listing.
/// </summary>
public sealed record GetCodeInfosRequest(
    int Page = 1,
    int Size = 100,
    string? ParentId = null);
