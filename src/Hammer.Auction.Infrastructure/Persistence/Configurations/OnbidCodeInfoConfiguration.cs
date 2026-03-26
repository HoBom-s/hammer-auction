using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="OnbidCodeInfo"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class OnbidCodeInfoConfiguration : IEntityTypeConfiguration<OnbidCodeInfo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OnbidCodeInfo> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.CtgrId)
            .IsUnique();

        builder.Property(e => e.CtgrId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CtgrNm).IsRequired().HasMaxLength(500);
        builder.Property(e => e.CtgrHirkId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CtgrHirkNm).IsRequired().HasMaxLength(500);
    }
}
