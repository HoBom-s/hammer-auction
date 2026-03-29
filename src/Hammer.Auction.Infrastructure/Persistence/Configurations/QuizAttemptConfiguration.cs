using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="QuizAttempt"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired().HasMaxLength(100);

        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(e => e.QuizId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.QuizId });
    }
}
