using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class CallUserConfiguration : IEntityTypeConfiguration<CallUser>
    {
        public static CallUserConfiguration Default => new CallUserConfiguration();

        public void Configure(EntityTypeBuilder<CallUser> builder)
        {
            builder.HasKey(e => new { e.Id, e.UserRole, e.CallRecordId } );
            builder.Property(a => a.UserRole)
                .HasMaxLength(50);

            builder.Property(a => a.DisplayName)
                .HasMaxLength(400);

            builder.Property(e => e.UserRole)
                .HasConversion<string>();

            builder.HasOne(a => a.CallRecord)
                .WithMany(a => a.CallUsers)
                .HasForeignKey(a => a.CallRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => i.DisplayName);
            builder.HasIndex(i => i.UserRole);
        }
    }
}
