namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     Real estate trade record from the MOLIT (국토교통부) API.
/// </summary>
public sealed class RealEstateTrade
{
    private RealEstateTrade()
    {
    }

    /// <summary>Gets the surrogate primary key.</summary>
    public long Id { get; init; }

    /// <summary>Gets the district code (법정동 시군구 코드, 5자리).</summary>
    public string LawdCd { get; private set; } = string.Empty;

    /// <summary>Gets the property type (0=Unknown, 1=Apartment, 2=Detached, 3=RowHouse, 4=Officetel, 5=Land, 6=Commercial).</summary>
    public int PropertyType { get; private set; }

    /// <summary>Gets the building/complex name (단지명).</summary>
    public string? BuildingName { get; private set; }

    /// <summary>Gets the lot number (지번).</summary>
    public string Jibun { get; private set; } = string.Empty;

    /// <summary>Gets the district name (법정동).</summary>
    public string UmdNm { get; private set; } = string.Empty;

    /// <summary>Gets the deal amount in 만원 (10,000 KRW).</summary>
    public long DealAmount { get; private set; }

    /// <summary>Gets the deal year.</summary>
    public int DealYear { get; private set; }

    /// <summary>Gets the deal month.</summary>
    public int DealMonth { get; private set; }

    /// <summary>Gets the deal day.</summary>
    public int DealDay { get; private set; }

    /// <summary>Gets the area in square meters (전용면적/거래면적/대지면적).</summary>
    public decimal Area { get; private set; }

    /// <summary>Gets the floor number.</summary>
    public int? Floor { get; private set; }

    /// <summary>Gets the build year.</summary>
    public int? BuildYear { get; private set; }

    /// <summary>Gets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the last update timestamp.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    ///     Creates a new real estate trade record.
    /// </summary>
    public static RealEstateTrade Create(
        string lawdCd,
        int propertyType,
        string? buildingName,
        string jibun,
        string umdNm,
        long dealAmount,
        int dealYear,
        int dealMonth,
        int dealDay,
        decimal area,
        int? floor,
        int? buildYear)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new RealEstateTrade
        {
            LawdCd = lawdCd,
            PropertyType = propertyType,
            BuildingName = buildingName,
            Jibun = jibun,
            UmdNm = umdNm,
            DealAmount = dealAmount,
            DealYear = dealYear,
            DealMonth = dealMonth,
            DealDay = dealDay,
            Area = area,
            Floor = floor,
            BuildYear = buildYear,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>
    ///     Updates mutable fields from a snapshot.
    /// </summary>
    public void UpdateFromSnapshot(
        string? buildingName,
        string umdNm,
        long dealAmount,
        int? floor,
        int? buildYear)
    {
        BuildingName = buildingName;
        UmdNm = umdNm;
        DealAmount = dealAmount;
        Floor = floor;
        BuildYear = buildYear;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
