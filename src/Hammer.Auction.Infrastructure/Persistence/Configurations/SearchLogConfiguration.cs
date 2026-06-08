using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="SearchLog"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SearchLogConfiguration : IEntityTypeConfiguration<SearchLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SearchLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Keyword).IsRequired().HasMaxLength(200);
        builder.Property(e => e.UserId).IsRequired().HasMaxLength(100);

        builder.HasIndex(e => e.SearchedAt);
        builder.HasIndex(e => new { e.UserId, e.SearchedAt });
        builder.HasIndex(e => e.Keyword);
    }
}
