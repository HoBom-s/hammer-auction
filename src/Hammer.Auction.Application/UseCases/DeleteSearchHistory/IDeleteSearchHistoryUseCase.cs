namespace Hammer.Auction.Application.UseCases.DeleteSearchHistory;

/// <summary>
/// Use case contract for deleting a user's search history.
/// </summary>
public interface IDeleteSearchHistoryUseCase
{
    /// <summary>
    /// Deletes all search logs for the given user.
    /// </summary>
    public Task<int> ExecuteAsync(string userId, CancellationToken ct = default);
}
