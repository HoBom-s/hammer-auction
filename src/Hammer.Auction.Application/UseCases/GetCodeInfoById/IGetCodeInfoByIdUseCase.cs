using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetCodeInfoById;

/// <summary>
/// Use case contract for retrieving a single code info entry by ID.
/// </summary>
public interface IGetCodeInfoByIdUseCase
{
    /// <summary>
    /// Retrieves a code info entry by its surrogate ID.
    /// </summary>
    public Task<OnbidCodeInfoResponse> ExecuteAsync(OnbidCodeInfoId id, CancellationToken ct = default);
}
