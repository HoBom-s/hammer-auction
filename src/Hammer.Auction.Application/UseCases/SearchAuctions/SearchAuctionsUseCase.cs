using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.SearchAuctions;

/// <summary>
/// Searches auction items from both KAMCO and institution sources.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class SearchAuctionsUseCase(
    IKamcoAuctionItemRepository kamcoRepo,
    IInstitutionAuctionItemRepository institutionRepo,
    ISearchLogRepository searchLogRepo) : ISearchAuctionsUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<UnifiedAuctionItemResponse>> ExecuteAsync(
        SearchAuctionsRequest request,
        string userId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        PageHelper.Validate(request.Page, request.Size);

        var keyword = request.Keyword?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new PagedResponse<UnifiedAuctionItemResponse>(
                [],
                request.Page,
                request.Size,
                0,
                0);
        }

        var fetchLimit = request.Page * request.Size;

        (IReadOnlyList<KamcoAuctionItem> Items, int TotalCount) kamcoResult =
            await kamcoRepo.SearchAsync(keyword, fetchLimit, ct);
        (IReadOnlyList<InstitutionAuctionItem> Items, int TotalCount) institutionResult =
            await institutionRepo.SearchAsync(keyword, fetchLimit, ct);

        var totalCount = kamcoResult.TotalCount + institutionResult.TotalCount;
        var totalPages = PageHelper.CalculateTotalPages(totalCount, request.Size);

        var merged = kamcoResult.Items
            .Select(UnifiedAuctionItemResponse.FromKamco)
            .Concat(institutionResult.Items.Select(UnifiedAuctionItemResponse.FromInstitution))
            .OrderByDescending(x => x.PbctBegnDtm)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToList();

        // Log search keyword
        var log = SearchLog.Create(keyword, userId);
        searchLogRepo.Add(log);
        await searchLogRepo.SaveChangesAsync(ct);

        return new PagedResponse<UnifiedAuctionItemResponse>(
            merged,
            request.Page,
            request.Size,
            totalCount,
            totalPages);
    }
}
