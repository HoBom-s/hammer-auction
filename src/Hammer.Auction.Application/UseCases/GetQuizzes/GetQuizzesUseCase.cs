using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetQuizzes;

/// <summary>
/// Retrieves a paginated list of quiz questions.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetQuizzesUseCase(IQuizRepository repository) : IGetQuizzesUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<QuizResponse>> ExecuteAsync(int page, int size, CancellationToken ct = default)
    {
        PageHelper.Validate(page, size);

        (IReadOnlyList<Quiz> items, var totalCount) = await repository.GetPagedAsync(page, size, ct);

        var responses = items.Select(QuizResponse.FromEntity).ToList();
        var totalPages = PageHelper.CalculateTotalPages(totalCount, size);

        return new PagedResponse<QuizResponse>(responses, page, size, totalCount, totalPages);
    }
}
