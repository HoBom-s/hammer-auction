using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.CreateQuiz;

/// <summary>
/// Creates a new quiz question and persists it.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class CreateQuizUseCase(IQuizRepository repository) : ICreateQuizUseCase
{
    /// <inheritdoc />
    public async Task<QuizResponse> ExecuteAsync(CreateQuizRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var quiz = Quiz.Create(
            request.Question,
            request.Choice1,
            request.Choice2,
            request.Choice3,
            request.Choice4,
            request.CorrectIndex,
            request.Explanation);

        repository.Add(quiz);
        await repository.SaveChangesAsync(ct);

        return QuizResponse.FromEntity(quiz);
    }
}
