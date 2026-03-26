using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetCodeInfos;

/// <summary>
/// Retrieves a paginated list of Onbid code info entries.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetCodeInfosUseCase(IOnbidCodeInfoRepository repository) : IGetCodeInfosUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<OnbidCodeInfoResponse>> ExecuteAsync(
        GetCodeInfosRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Entities.OnbidCodeInfo>, int) result = await repository.GetPagedAsync(
            request.Page,
            request.Size,
            request.ParentId,
            ct);

        var responses = result.Item1.Select(OnbidCodeInfoResponse.FromEntity).ToList();
        var totalPages = (int)Math.Ceiling((double)result.Item2 / request.Size);

        return new PagedResponse<OnbidCodeInfoResponse>(
            responses,
            request.Page,
            request.Size,
            result.Item2,
            totalPages);
    }
}
