using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.ModelBinding;

/// <summary>
///     Gateway가 전달한 X-User-Id 헤더에서 사용자 식별자를 바인딩합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class CurrentUserIdAttribute : ModelBinderAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentUserIdAttribute"/> class.
    /// </summary>
    public CurrentUserIdAttribute()
        : base(typeof(CurrentUserIdModelBinder))
    {
    }
}
