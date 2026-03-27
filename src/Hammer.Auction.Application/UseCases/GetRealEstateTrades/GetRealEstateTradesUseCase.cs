using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetRealEstateTrades;

/// <summary>
/// Retrieves a paginated list of real estate trade records.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetRealEstateTradesUseCase(IRealEstateTradeRepository repository) : IGetRealEstateTradesUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<RealEstateTradeResponse>> ExecuteAsync(
        GetRealEstateTradesRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Entities.RealEstateTrade>, int) result = await repository.GetPagedAsync(
            request.Page,
            request.Size,
            request.LawdCd,
            request.PropertyType,
            ct);

        var responses = result.Item1.Select(RealEstateTradeResponse.FromEntity).ToList();
        var totalPages = (int)Math.Ceiling((double)result.Item2 / request.Size);

        return new PagedResponse<RealEstateTradeResponse>(
            responses,
            request.Page,
            request.Size,
            result.Item2,
            totalPages);
    }
}
