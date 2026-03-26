using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetCodeInfoById;
using Hammer.Auction.Application.UseCases.GetCodeInfos;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
/// Provides endpoints for Onbid code info entries.
/// </summary>
[ApiController]
[Route("code-infos")]
[Tags("CodeInfo")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class CodeInfoController(
    IGetCodeInfosUseCase getCodeInfos,
    IGetCodeInfoByIdUseCase getCodeInfoById) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of code info entries.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<OnbidCodeInfoResponse>>> GetCodeInfosAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? parentId = null,
        CancellationToken ct = default)
    {
        GetCodeInfosRequest request = new(page, size, parentId);
        PagedResponse<OnbidCodeInfoResponse> result = await getCodeInfos.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single code info entry by its ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OnbidCodeInfoResponse>> GetCodeInfoByIdAsync(long id, CancellationToken ct = default)
    {
        OnbidCodeInfoResponse result = await getCodeInfoById.ExecuteAsync(id, ct);

        return Ok(result);
    }
}
