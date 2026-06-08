using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetCodeInfos;

/// <summary>
/// Use case contract for retrieving paginated code info entries.
/// </summary>
public interface IGetCodeInfosUseCase
{
    /// <summary>
    /// Retrieves a paginated list of code info entries.
    /// </summary>
    public Task<PagedResponse<OnbidCodeInfoResponse>> ExecuteAsync(
        GetCodeInfosRequest request,
        CancellationToken ct = default);
}
