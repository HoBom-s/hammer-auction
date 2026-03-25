using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="KamcoAuctionItem"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class KamcoAuctionItemConfiguration : IEntityTypeConfiguration<KamcoAuctionItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<KamcoAuctionItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.PlnmNo, e.PbctNo, e.CltrNo })
            .IsUnique();

        builder.Property(e => e.CltrNm).IsRequired().HasMaxLength(500);
        builder.Property(e => e.CtgrFullNm).IsRequired().HasMaxLength(500);
        builder.Property(e => e.LdnmAdrs).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.NmrdAdrs).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.BidMtdNm).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PbctCltrStatNm).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CltrImgFiles).HasMaxLength(2048);

        builder.HasIndex(e => e.PbctClsDtm);
        builder.HasIndex(e => e.PbctCltrStatNm);
        builder.HasIndex(e => e.MinBidPrc);
        builder.HasIndex(e => e.CtgrFullNm);
    }
}
