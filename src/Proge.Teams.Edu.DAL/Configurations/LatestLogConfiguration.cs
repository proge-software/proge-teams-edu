using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class LatestLogConfiguration : IEntityTypeConfiguration<LatestLog>
    {
        public static LatestLogConfiguration Default => new LatestLogConfiguration();

        public void Configure(EntityTypeBuilder<LatestLog> builder)
        {
            builder.HasNoKey();
            builder.ToView("LatestLog");
        }

    }
}
