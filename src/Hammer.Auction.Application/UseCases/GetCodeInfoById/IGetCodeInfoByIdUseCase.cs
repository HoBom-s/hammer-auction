using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetCodeInfoById;

/// <summary>
/// Use case contract for retrieving a single code info entry by ID.
/// </summary>
public interface IGetCodeInfoByIdUseCase
{
    /// <summary>
    /// Retrieves a code info entry by its surrogate ID.
    /// </summary>
    public Task<OnbidCodeInfoResponse> ExecuteAsync(long id, CancellationToken ct = default);
}
