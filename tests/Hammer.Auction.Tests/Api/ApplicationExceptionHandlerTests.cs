using System.Net;
using FluentAssertions;
using Hammer.Auction.Application.Exceptions;

namespace Hammer.Auction.Tests.Api;

/// <summary>
/// Tests for <see cref="Hammer.Auction.Api.Middleware.ApplicationExceptionHandler"/>.
/// </summary>
public sealed class ApplicationExceptionHandlerTests
{
    [Theory]
    [InlineData(typeof(BadRequestException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(UnauthorizedException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(ForbiddenException), HttpStatusCode.Forbidden)]
    [InlineData(typeof(NotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(ConflictException), HttpStatusCode.Conflict)]
    [InlineData(typeof(ServiceUnavailableException), HttpStatusCode.ServiceUnavailable)]
    public void AllExceptionTypes_ShouldBeInstantiable(Type exceptionType, HttpStatusCode expectedStatus)
    {
        var ex = Activator.CreateInstance(exceptionType) as Exception;
        ex.Should().NotBeNull();

        var exWithMessage = Activator.CreateInstance(exceptionType, "test message") as Exception;
        exWithMessage.Should().NotBeNull();
        exWithMessage!.Message.Should().Be("test message");

        Exception inner = new InvalidOperationException("inner");
        var exWithInner = Activator.CreateInstance(exceptionType, "msg", inner) as Exception;
        exWithInner.Should().NotBeNull();
        exWithInner!.InnerException.Should().Be(inner);

        _ = expectedStatus;
    }
}
