using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;

/// <summary>
/// Retrieves a paginated list of institution auction items.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetInstitutionAuctionItemsUseCase(IInstitutionAuctionItemRepository repository) : IGetInstitutionAuctionItemsUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<InstitutionAuctionItemResponse>> ExecuteAsync(
        GetInstitutionAuctionItemsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Entities.InstitutionAuctionItem>, int) result = await repository.GetPagedAsync(
            request.Page,
            request.Size,
            request.Org,
            request.Category,
            request.Keyword,
            ct);

        var responses = result.Item1.Select(InstitutionAuctionItemResponse.FromEntity).ToList();
        var totalPages = (int)Math.Ceiling((double)result.Item2 / request.Size);

        return new PagedResponse<InstitutionAuctionItemResponse>(
            responses,
            request.Page,
            request.Size,
            result.Item2,
            totalPages);
    }
}
