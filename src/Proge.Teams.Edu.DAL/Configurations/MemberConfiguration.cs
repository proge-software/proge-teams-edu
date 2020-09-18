using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public static MemberConfiguration Default => new MemberConfiguration();

        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.HasKey(o => o.MemberId);

            builder.Property(a => a.UserPrincipalName)                
                .HasMaxLength(200);

            builder.Property(a => a.DisplayName)
                .HasMaxLength(400);

            builder.Property(a => a.Mail)
                .HasMaxLength(300);

            builder.Property(a => a.OfficeLocation)
                .HasMaxLength(300);

            builder.Property(a => a.JobTitle)
                .HasMaxLength(300);

            builder.HasIndex(a => a.UserPrincipalName);

        }

    }
}
