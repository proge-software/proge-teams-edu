using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class TeamsMeetingConfiguration : IEntityTypeConfiguration<TeamsMeeting>
    {
        public static TeamsMeetingConfiguration Default => new TeamsMeetingConfiguration();

        public void Configure(EntityTypeBuilder<TeamsMeeting> builder)
        {
            builder.HasKey(k => k.JoinUrl);

            builder.Property(p => p.MeetingName)
                .HasMaxLength(450);

            builder.Property(p => p.JoinUrl)
                .HasMaxLength(450);

            builder.Property(p => p.MeetingId)
                .HasMaxLength(150);
        }
    }
}
