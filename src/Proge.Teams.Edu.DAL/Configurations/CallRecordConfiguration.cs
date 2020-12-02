using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class CallRecordConfiguration : IEntityTypeConfiguration<CallRecord>
    {
        public static CallRecordConfiguration Default => new CallRecordConfiguration();

        public void Configure(EntityTypeBuilder<CallRecord> builder)
        {
            builder.HasKey(e => new { e.Id });                   


            builder.Property(a => a.Type)
                .HasMaxLength(50);

            builder.Property(a => a.CallDescription)
                .HasMaxLength(450);

            builder.Property(a => a.Modalities)
                .HasMaxLength(200);

            builder.HasIndex(i => i.Id);
            builder.HasIndex(i => i.JoinWebUrl);
            builder.HasIndex(i => i.Type);
        }
    }
}