using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Quiz"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Question).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Choice1).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Choice2).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Choice3).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Choice4).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Explanation).IsRequired().HasMaxLength(1000);
    }
}
