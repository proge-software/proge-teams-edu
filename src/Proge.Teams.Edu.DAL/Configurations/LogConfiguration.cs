using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class LogConfiguration : IEntityTypeConfiguration<Log>
    {
        public static LogConfiguration Default => new LogConfiguration();

        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.HasKey(o => o.Id)
                .IsClustered()
                ;

            builder.Property(a => a.Id)
                .UseIdentityColumn();

            builder.Property(a => a.Message)
                .HasMaxLength(int.MaxValue);

            builder.Property(a => a.MessageTemplate)
                .HasMaxLength(int.MaxValue);

            builder.Property(a => a.Level)
                .HasMaxLength(128);

            builder.Property(a => a.TimeStamp)
                .HasColumnType("datetimeoffset(7)")
                ;

            builder.Property(a => a.Exception)
                .HasMaxLength(int.MaxValue);

            builder.Property(a => a.Properties)
                .HasColumnType("xml");

            builder.Ignore(c => c.PropertiesWrapper);

            builder.Property(a => a.LogEvent)
                .HasMaxLength(int.MaxValue);

            builder.HasIndex(a => a.Level);
            builder.HasIndex(a => a.TimeStamp);

        }

    }
}
