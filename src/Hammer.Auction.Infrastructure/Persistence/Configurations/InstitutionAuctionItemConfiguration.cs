using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="InstitutionAuctionItem"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class InstitutionAuctionItemConfiguration : IEntityTypeConfiguration<InstitutionAuctionItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InstitutionAuctionItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.PlnmNo, e.PbctNo })
            .IsUnique();

        builder.Property(e => e.PlnmKindCd).IsRequired().HasMaxLength(50);
        builder.Property(e => e.PlnmKindNm).IsRequired().HasMaxLength(200);
        builder.Property(e => e.BidDvsnCd).IsRequired().HasMaxLength(50);
        builder.Property(e => e.BidDvsnNm).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PlnmNm).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.OrgNm).IsRequired().HasMaxLength(500);
        builder.Property(e => e.PlnmDt).IsRequired().HasMaxLength(8);
        builder.Property(e => e.OrgPlnmNo).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PlnmMnmtNo).IsRequired().HasMaxLength(200);
        builder.Property(e => e.BidMtdCd).IsRequired().HasMaxLength(50);
        builder.Property(e => e.BidMtdNm).IsRequired().HasMaxLength(200);
        builder.Property(e => e.TotAmtUnpcDvsnCd).IsRequired().HasMaxLength(50);
        builder.Property(e => e.TotAmtUnpcDvsnNm).IsRequired().HasMaxLength(200);
        builder.Property(e => e.DpslMtdCd).IsRequired().HasMaxLength(50);
        builder.Property(e => e.DpslMtdNm).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PrptDvsnCd).IsRequired().HasMaxLength(50);
        builder.Property(e => e.PrptDvsnNm).IsRequired().HasMaxLength(200);
        builder.Property(e => e.CtgrId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CtgrFullNm).IsRequired().HasMaxLength(500);

        builder.HasIndex(e => e.PbctClsDtm);
        builder.HasIndex(e => e.CtgrId);
        builder.HasIndex(e => e.OrgNm);
    }
}
