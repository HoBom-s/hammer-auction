using Hammer.Auction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Hammer.Auction.Api.ModelBinding;

/// <summary>
///     X-User-Id 헤더에서 <see cref="UserId"/> 값 객체를 추출하는 모델 바인더.
/// </summary>
internal sealed class CurrentUserIdModelBinder : IModelBinder
{
    private const string HeaderName = "X-User-Id";

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var headerValue = bindingContext.HttpContext.Request.Headers[HeaderName].ToString();

        UserId userId = string.IsNullOrWhiteSpace(headerValue)
            ? UserId.Anonymous
            : new UserId(headerValue);

        bindingContext.Result = ModelBindingResult.Success(userId);

        return Task.CompletedTask;
    }
}
