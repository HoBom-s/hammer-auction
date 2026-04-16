namespace Hammer.Auction.Application;

/// <summary>
/// Configuration for the periodic data cleanup worker.
/// </summary>
public sealed class CleanupSettings
{
    /// <summary>Gets or sets the retention period in days for closed KAMCO auction items.</summary>
    public int KamcoRetentionDays { get; set; } = 90;

    /// <summary>Gets or sets the retention period in days for closed institution auction items.</summary>
    public int InstitutionRetentionDays { get; set; } = 90;

    /// <summary>Gets or sets the retention period in days for real estate trade records.</summary>
    public int RealEstateTradeRetentionDays { get; set; } = 1095;

    /// <summary>Gets or sets the retention period in days for search log entries.</summary>
    public int SearchLogRetentionDays { get; set; } = 30;

    /// <summary>Gets or sets the hour (UTC) at which cleanup runs daily. Default 18 = KST 03:00.</summary>
    public int CleanupHourUtc { get; set; } = 18;
}
