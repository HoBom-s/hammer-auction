using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
///     아웃박스 메시지를 변경 추적기에 추가한다. SaveChanges와 함께 커밋된다.
/// </summary>
public interface IOutboxWriter
{
    public void Enqueue(OutboxMessage message);
}
