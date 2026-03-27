using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="RealEstateTrade"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class RealEstateTradeConfiguration : IEntityTypeConfiguration<RealEstateTrade>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RealEstateTrade> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.LawdCd, e.PropertyType, e.Jibun, e.DealYear, e.DealMonth, e.DealDay, e.Area })
            .IsUnique();

        builder.Property(e => e.LawdCd).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Jibun).IsRequired().HasMaxLength(100);
        builder.Property(e => e.UmdNm).IsRequired().HasMaxLength(100);
        builder.Property(e => e.BuildingName).HasMaxLength(200);
        builder.Property(e => e.Area).HasPrecision(18, 4);

        builder.HasIndex(e => e.LawdCd);
        builder.HasIndex(e => e.PropertyType);
        builder.HasIndex(e => new { e.DealYear, e.DealMonth, e.DealDay });
    }
}
