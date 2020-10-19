using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class ChangeNotificationConfiguration : IEntityTypeConfiguration<ChangeNotification>
    {
        public static ChangeNotificationConfiguration Default => new ChangeNotificationConfiguration();

        public void Configure(EntityTypeBuilder<ChangeNotification> builder)
        {
            builder.HasKey(o => new { o.Id, o.ODataId, o.ChangeType });

            builder.Property(a => a.RawJson)
                .HasMaxLength(int.MaxValue);

            builder.Property(a => a.ChangeType)
                .HasConversion<string>();

            builder.Property(a => a.SubscriptionExpirationDateTime)
                .HasColumnType("datetimeoffset(7)");

            builder.HasIndex(a => a.ChangeType);

            builder.Property(a => a.Resource)
                .HasMaxLength(400);

            builder.Property(a => a.ODataType)
                .HasMaxLength(400);

        }
    }
}