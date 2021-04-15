using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;

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

            builder.Property(p => p.MeetingIdPrimary)
                .HasMaxLength(20);

            builder.Property(p => p.MeetingIdSecondary)
                .HasMaxLength(20);

            builder.Property(p => p.MeetingExtendedName)
                .HasMaxLength(255);

            builder.Property(p => p.OwnerExtended)
                .HasMaxLength(255);

            builder.Property(p => p.OwnerUpn)
                .HasMaxLength(255);

            builder.Property(p => p.MeetingExtendedAttribute)
                .HasMaxLength(10);

            builder.Property(p => p.MeetingType)
                .HasMaxLength(255);

            builder.Property(p => p.MeetingHierarchy)
                .HasMaxLength(10);

            builder.Property(p => p.CustomAttribute);

            builder.Property(p => p.Attendees);
            builder.Property(p => p.ChatThreadId)
                .HasMaxLength(255);

            builder.Property(p => p.ChatMessageId)
                .HasMaxLength(255);

            builder.Property(p => p.CreationDateTime);

            // Indices
            builder.HasIndex(i => i.MeetingId);
        }
    }
}
