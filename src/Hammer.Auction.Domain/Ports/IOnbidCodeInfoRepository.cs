using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for Onbid code info entries.
/// </summary>
public interface IOnbidCodeInfoRepository
{
    /// <summary>
    /// Finds an entry by its surrogate primary key.
    /// </summary>
    public Task<OnbidCodeInfo?> GetByIdAsync(OnbidCodeInfoId id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated list of entries with optional filtering.
    /// </summary>
    public Task<(IReadOnlyList<OnbidCodeInfo> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? parentId,
        CancellationToken ct = default);
}
