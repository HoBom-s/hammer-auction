using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.DeleteQuiz;

/// <summary>
/// Deletes a quiz question by its identifier.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class DeleteQuizUseCase(IQuizRepository repository) : IDeleteQuizUseCase
{
    /// <inheritdoc />
    public async Task ExecuteAsync(QuizId id, CancellationToken ct = default)
    {
        Quiz quiz = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Quiz {id} not found.");

        repository.Remove(quiz);
        await repository.SaveChangesAsync(ct);
    }
}
