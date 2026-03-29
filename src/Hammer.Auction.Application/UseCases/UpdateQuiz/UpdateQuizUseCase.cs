using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.UpdateQuiz;

/// <summary>
/// Updates an existing quiz question.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class UpdateQuizUseCase(IQuizRepository repository) : IUpdateQuizUseCase
{
    /// <inheritdoc />
    public async Task<QuizResponse> ExecuteAsync(QuizId id, UpdateQuizRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Quiz quiz = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Quiz {id} not found.");

        quiz.Update(
            request.Question,
            request.Choice1,
            request.Choice2,
            request.Choice3,
            request.Choice4,
            request.CorrectIndex,
            request.Explanation);

        await repository.SaveChangesAsync(ct);

        return QuizResponse.FromEntity(quiz);
    }
}
