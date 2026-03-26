using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.GetCodeInfoById;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetCodeInfoByIdUseCase"/>.
/// </summary>
public sealed class GetCodeInfoByIdUseCaseTests
{
    private readonly IOnbidCodeInfoRepository _repository = Substitute.For<IOnbidCodeInfoRepository>();
    private readonly GetCodeInfoByIdUseCase _sut;

    public GetCodeInfoByIdUseCaseTests()
    {
        _sut = new GetCodeInfoByIdUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingItem_ShouldReturnResponseAsync()
    {
        OnbidCodeInfo entity = CreateEntity(1, "CTG01", "토지");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

        OnbidCodeInfoResponse result = await _sut.ExecuteAsync(1L);

        result.Should().NotBeNull();
        result.CtgrId.Should().Be("CTG01");
        result.CtgrNm.Should().Be("토지");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentItem_ShouldThrowNotFoundExceptionAsync()
    {
        _repository.GetByIdAsync(999L, Arg.Any<CancellationToken>()).Returns((OnbidCodeInfo?)null);

        Func<Task> act = () => _sut.ExecuteAsync(999L);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999*");
    }

    private static OnbidCodeInfo CreateEntity(long id, string ctgrId, string ctgrNm)
    {
        var item = OnbidCodeInfo.Create(ctgrId, ctgrNm, "ROOT", "전체");

        typeof(OnbidCodeInfo)
            .GetProperty(nameof(OnbidCodeInfo.Id))!
            .SetValue(item, id);

        return item;
    }
}
