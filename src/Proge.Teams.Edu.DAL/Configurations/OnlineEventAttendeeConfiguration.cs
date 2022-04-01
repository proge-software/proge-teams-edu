using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class OnlineEventAttendeeConfiguration : IEntityTypeConfiguration<OnlineEventAttendee>
    {
        public static OnlineEventAttendeeConfiguration Default => new OnlineEventAttendeeConfiguration();

        public void Configure(EntityTypeBuilder<OnlineEventAttendee> builder)
        {
            builder.HasKey(e => new { e.AttendeeMail, e.OnlineEventId, e.AttendeeType });

            builder.HasOne(d => d.Attendee)
               .WithMany(p => p.OnlineEventAttendees)
               .HasForeignKey(d => d.AttendeeMail)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
            ;

            builder.HasOne(d => d.OnlineEvent)
                .WithMany(p => p.OnlineEventAttendee)
                .HasForeignKey(d => d.OnlineEventId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            ;

            builder.Property(a => a.AttendeeType)
                .HasConversion<string>();

            builder.HasIndex(a => a.AttendeeType);
        }

    }
}
