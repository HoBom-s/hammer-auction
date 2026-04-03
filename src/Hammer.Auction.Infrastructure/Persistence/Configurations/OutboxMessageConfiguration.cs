using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

[ExcludeFromCodeCoverage]
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Topic).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Key).HasMaxLength(256);
        builder.Property(e => e.Payload).IsRequired();

        builder.HasIndex(e => e.ProcessedAt)
            .HasFilter("processed_at IS NULL");

        builder.HasIndex(e => e.CreatedAt);
    }
}
