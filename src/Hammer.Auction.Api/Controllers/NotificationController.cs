using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Api.ModelBinding;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetNotifications;
using Hammer.Auction.Application.UseCases.GetNotificationSettings;
using Hammer.Auction.Application.UseCases.GetUnreadNotificationCount;
using Hammer.Auction.Application.UseCases.ReadAllNotifications;
using Hammer.Auction.Application.UseCases.ReadNotification;
using Hammer.Auction.Application.UseCases.UpdateNotificationSettings;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     알림 API.
///     알림 목록 조회, 읽음 처리, 설정 관리를 처리합니다.
/// </summary>
[ApiController]
[Route("notifications")]
[Tags("Notification")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
public sealed class NotificationController(
    IGetNotificationsUseCase getNotifications,
    IGetUnreadNotificationCountUseCase getUnreadCount,
    IReadNotificationUseCase readNotification,
    IReadAllNotificationsUseCase readAllNotifications,
    IGetNotificationSettingsUseCase getSettings,
    IUpdateNotificationSettingsUseCase updateSettings) : ControllerBase
{
    /// <summary>
    ///     알림 목록을 조회합니다 (페이지네이션).
    /// </summary>
    /// <param name="page">페이지 번호 (기본값: 1).</param>
    /// <param name="size">페이지 크기 (기본값: 20).</param>
    /// <param name="userId">사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<PagedResponse<NotificationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<NotificationResponse>>> GetListAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        PagedResponse<NotificationResponse> result = await getNotifications.ExecuteAsync(
            userId,
            new GetNotificationsRequest(page, size),
            ct);

        return Ok(result);
    }

    /// <summary>
    ///     읽지 않은 알림 수를 조회합니다.
    /// </summary>
    /// <param name="userId">사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("unread-count")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCountAsync(
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        var count = await getUnreadCount.ExecuteAsync(userId, ct);

        return Ok(count);
    }

    /// <summary>
    ///     알림을 읽음 처리합니다.
    /// </summary>
    /// <param name="id">알림 식별자.</param>
    /// <param name="userId">사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPatch("{id:long}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReadAsync(
        long id,
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        await readNotification.ExecuteAsync(userId, id, ct);

        return NoContent();
    }

    /// <summary>
    ///     모든 알림을 읽음 처리합니다.
    /// </summary>
    /// <param name="userId">사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReadAllAsync(
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        await readAllNotifications.ExecuteAsync(userId, ct);

        return NoContent();
    }

    /// <summary>
    ///     알림 설정을 조회합니다.
    /// </summary>
    /// <param name="userId">사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("settings")]
    [ProducesResponseType<NotificationSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationSettingsResponse>> GetSettingsAsync(
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        NotificationSettingsResponse result = await getSettings.ExecuteAsync(userId, ct);

        return Ok(result);
    }

    /// <summary>
    ///     알림 설정을 변경합니다.
    /// </summary>
    /// <param name="request">설정 변경 요청.</param>
    /// <param name="userId">사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("settings")]
    [ProducesResponseType<NotificationSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationSettingsResponse>> UpdateSettingsAsync(
        [FromBody] UpdateNotificationSettingsRequest request,
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        NotificationSettingsResponse result = await updateSettings.ExecuteAsync(userId, request, ct);

        return Ok(result);
    }
}
