using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetCodeInfoById;

/// <summary>
/// Retrieves a single Onbid code info entry by its surrogate ID.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetCodeInfoByIdUseCase(IOnbidCodeInfoRepository repository) : IGetCodeInfoByIdUseCase
{
    /// <inheritdoc />
    public async Task<OnbidCodeInfoResponse> ExecuteAsync(long id, CancellationToken ct = default)
    {
        Domain.Entities.OnbidCodeInfo item = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Code info with ID {id} was not found.");

        return OnbidCodeInfoResponse.FromEntity(item);
    }
}
