using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetRealEstateTrades;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
/// Provides endpoints for real estate trade records.
/// </summary>
[ApiController]
[Route("real-estate-trades")]
[Tags("RealEstateTrade")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class RealEstateTradeController(
    IGetRealEstateTradesUseCase getRealEstateTrades) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of real estate trade records.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<RealEstateTradeResponse>>> GetRealEstateTradesAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? lawdCd = null,
        [FromQuery] int? propertyType = null,
        CancellationToken ct = default)
    {
        GetRealEstateTradesRequest request = new(page, size, lawdCd, propertyType);
        PagedResponse<RealEstateTradeResponse> result = await getRealEstateTrades.ExecuteAsync(request, ct);

        return Ok(result);
    }
}
