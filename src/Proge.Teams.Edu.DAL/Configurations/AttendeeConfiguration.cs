using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.DAL
{
    internal class AttendeeConfiguration : IEntityTypeConfiguration<Attendee>
    {
        public static AttendeeConfiguration Default => new AttendeeConfiguration();

        public void Configure(EntityTypeBuilder<Attendee> builder)
        {
            builder.HasKey(e => e.Email)
                .IsClustered();

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(450);
        }
    }
}
