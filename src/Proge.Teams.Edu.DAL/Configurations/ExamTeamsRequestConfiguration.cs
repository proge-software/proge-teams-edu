using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class ExamTeamsRequestConfiguration : IEntityTypeConfiguration<ExamTeamsRequest>
    {
        public static ExamTeamsRequestConfiguration Default => new ExamTeamsRequestConfiguration();

        public void Configure(EntityTypeBuilder<ExamTeamsRequest> builder)
        {
            builder
              .HasKey(e => new { e.Id });
            
            builder
              .Property(a => a.DipCod)
              .HasMaxLength(150);
            
            builder
              .Property(a => a.InternalId)
              .HasMaxLength(100);
            
            builder
              .Property(a => a.Ins_aa_offerta)
              .HasMaxLength(50);
            
            builder
              .Property(a => a.Ins_aa_ord)
              .HasMaxLength(50);
            
            builder
              .Property(a => a.Ins_ad_cod)
              .HasMaxLength(50);
            
            builder
              .Property(a => a.Ins_cds_cod)
              .HasMaxLength(50);
            
            builder
              .Property(a => a.App_cdsDes)
              .HasMaxLength(400);
            
            builder
              .Property(a => a.App_cdsCod)
              .HasMaxLength(50);
            
            builder
              .Property(a => a.ExternalId)
              .HasMaxLength(50);
            
            builder
              .Property(a => a.Ins_pds_cod)
              .HasMaxLength(50);
            
            builder
              .Property(a => a.AdditionalDataString);

            builder
             .Property(a => a.TeamJoinUrl)
             .HasMaxLength(450);

            builder
            .Property(a => a.TeamId)
            .HasMaxLength(50);

            builder
            .Property(a => a.TeamRequestUser)
            .HasMaxLength(50);
        }
    }
}
