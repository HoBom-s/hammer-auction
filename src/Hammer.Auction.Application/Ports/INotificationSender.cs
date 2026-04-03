using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.Ports;

/// <summary>
/// 알림 페이로드를 아웃박스에 추가한다. 호출자의 SaveChanges와 함께 커밋된다.
/// </summary>
public interface INotificationSender
{
    public void Send(NotificationPayload payload);
}
