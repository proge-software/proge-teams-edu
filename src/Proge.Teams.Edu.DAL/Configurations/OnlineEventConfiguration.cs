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
    public class OnlineEventConfiguration<T> : IEntityTypeConfiguration<T>
        where T : OnlineEvent
    {
        public static OnlineEventConfiguration<T> Default => new OnlineEventConfiguration<T>();

        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id)
                .IsClustered();

            builder.Property(a => a.Id)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(a => a.Subject)
                .IsRequired(false)
                .HasMaxLength(450);

            builder.Property(a => a.Location)
                .HasMaxLength(450);

            builder.Property(a => a.ChangeKey)
                .HasMaxLength(450);

            builder.Property(a => a.ICalUId)
                .HasMaxLength(450);

            builder.Property(a => a.JoinUrl)
                .HasMaxLength(450);

            builder.Property(a => a.WebLink)
                .HasMaxLength(450);

            builder.HasIndex(a => a.Subject);
            builder.HasIndex(a => a.JoinUrl);
            builder.HasIndex(a => a.StartDateTime);
        }
    }
}
