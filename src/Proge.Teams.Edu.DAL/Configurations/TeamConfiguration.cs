using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public static TeamConfiguration Default => new TeamConfiguration();

        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(o => o.TeamsId);

            builder.Property(a => a.InternalId)
                .HasMaxLength(100);

            builder.Property(a => a.ExternalId)
                .HasMaxLength(100);
            builder.Property(a => a.JoinCode)
                .HasMaxLength(30);

            builder.Property(a => a.JoinUrl)
                .HasMaxLength(450);

            builder.Property(a => a.TeamType)
                .HasMaxLength(200);

            builder.Property(a => a.TeamsId)
                .HasMaxLength(30);

            builder.Property(a => a.Name)
                .HasMaxLength(1000);

            builder.Property(a => a.Description)
                .HasMaxLength(1000);

            builder.Property(a => a.DepartmentId)
                .HasMaxLength(20);

            builder.Property(a => a.AdId)
                .HasMaxLength(20);

            builder.Property(a => a.AnnoOfferta)
                .HasMaxLength(10);

            builder.Property(a => a.AdCod)
                .HasMaxLength(10);

            builder.Property(a => a.AdDesc)
                .HasMaxLength(450);

            builder.Property(a => a.AnnoOrdinamento)
                .HasMaxLength(5);

            builder.Property(a => a.CdsCod)
                .HasMaxLength(450);
            builder
              .Property(a => a.AdditionalDataString);
            builder.HasIndex(a => a.ExternalId);
            builder.HasIndex(a => a.TeamsId);
            builder.HasIndex(a => a.ExternalId);
            builder.HasIndex(a => a.TeamType);
        }

    }
}
