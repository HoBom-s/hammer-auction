using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
/// Provides endpoints for institution auction items.
/// </summary>
[ApiController]
[Route("institution-auctions/items")]
[Tags("InstitutionAuction")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class InstitutionAuctionController(
    IGetInstitutionAuctionItemsUseCase getItems,
    IGetInstitutionAuctionItemByIdUseCase getItemById) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of institution auction items.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<InstitutionAuctionItemResponse>>> GetItemsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? org = null,
        [FromQuery] string? category = null,
        [FromQuery] string? keyword = null,
        CancellationToken ct = default)
    {
        GetInstitutionAuctionItemsRequest request = new(page, size, org, category, keyword);
        PagedResponse<InstitutionAuctionItemResponse> result = await getItems.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single institution auction item by its ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstitutionAuctionItemResponse>> GetItemByIdAsync(long id, CancellationToken ct = default)
    {
        InstitutionAuctionItemResponse result = await getItemById.ExecuteAsync(id, ct);

        return Ok(result);
    }
}
