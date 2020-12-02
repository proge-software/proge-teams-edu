using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class CallSegmentConfiguration : IEntityTypeConfiguration<CallSegment>
    {
        public static CallSegmentConfiguration Default => new CallSegmentConfiguration();

        public void Configure(EntityTypeBuilder<CallSegment> builder)
        {
            builder.HasKey(e => e.Id );

            builder.Property(a => a.CallUserRole)
                .HasMaxLength(50);

            builder.Property(a => a.UserPlatform)
                .HasMaxLength(100);

            builder.Property(a => a.UserProductFamily)
                .HasMaxLength(100);

            builder.Property(e => e.CallUserRole)
                .HasConversion<string>();

            builder.HasOne(a => a.CallSession)
                .WithMany(a => a.CallSegments)
                .HasForeignKey(a => a.CallSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => i.Id);
        }
    }
}
