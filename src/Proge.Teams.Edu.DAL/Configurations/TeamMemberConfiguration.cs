using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
    {
        public static TeamMemberConfiguration Default => new TeamMemberConfiguration();

        public void Configure(EntityTypeBuilder<TeamMember> builder)
        {
            builder.HasKey(e => new { e.TeamId, e.MemberId, e.MemberType });

            builder.HasOne(d => d.Member)
               .WithMany(p => p.TeamsUsers)
               .HasForeignKey(d => d.MemberId)
               .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            ;

            builder.HasOne(d => d.Team)
                .WithMany(p => p.TeamsUsers)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();
            ;

            builder.Property(a => a.MemberType)
                .HasConversion<string>();

            builder.HasIndex(a => a.MemberType);
        }

    }
}
