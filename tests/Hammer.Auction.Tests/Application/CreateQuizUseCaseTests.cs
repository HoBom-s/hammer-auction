using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.CreateQuiz;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="CreateQuizUseCase"/>.
/// </summary>
public sealed class CreateQuizUseCaseTests
{
    private readonly IQuizRepository _repository = Substitute.For<IQuizRepository>();
    private readonly CreateQuizUseCase _sut;

    public CreateQuizUseCaseTests()
    {
        _sut = new CreateQuizUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldCreateAndReturnQuizAsync()
    {
        CreateQuizRequest request = new("Question?", "A", "B", "C", "D", 2, "Explanation");

        QuizResponse result = await _sut.ExecuteAsync(request);

        result.Question.Should().Be("Question?");
        result.Choices.Should().Equal("A", "B", "C", "D");
        result.CorrectIndex.Should().Be(2);
        result.Explanation.Should().Be("Explanation");

        _repository.Received(1).Add(Arg.Any<Quiz>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullExceptionAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
