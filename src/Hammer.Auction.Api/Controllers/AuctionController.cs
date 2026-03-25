using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetAuctionItemById;
using Hammer.Auction.Application.UseCases.GetAuctionItems;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
/// Provides endpoints for KAMCO auction items.
/// </summary>
[ApiController]
[Route("hammer-auctions/items")]
[Tags("Auction")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class AuctionController(
    IGetAuctionItemsUseCase getAuctionItems,
    IGetAuctionItemByIdUseCase getAuctionItemById) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of auction items.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<KamcoAuctionItemResponse>>> GetItemsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] string? keyword = null,
        CancellationToken ct = default)
    {
        GetAuctionItemsRequest request = new(page, size, status, category, keyword);
        PagedResponse<KamcoAuctionItemResponse> result = await getAuctionItems.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single auction item by its ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KamcoAuctionItemResponse>> GetItemByIdAsync(long id, CancellationToken ct = default)
    {
        KamcoAuctionItemResponse result = await getAuctionItemById.ExecuteAsync(id, ct);

        return Ok(result);
    }
}
