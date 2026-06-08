using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Auction.Infrastructure.Persistence.Configurations;

/// <summary>
///     EF Core configuration for the <see cref="NotificationSetting" /> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class NotificationSettingConfiguration : IEntityTypeConfiguration<NotificationSetting>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationSetting> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IsEnabled).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.UserId).IsUnique();
    }
}
