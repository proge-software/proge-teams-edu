using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Configurations
{
    internal class TeamWithMemeberConfiguration : IEntityTypeConfiguration<TeamWithMemeber>
    {
        public static TeamWithMemeberConfiguration Default => new TeamWithMemeberConfiguration();

        public void Configure(EntityTypeBuilder<TeamWithMemeber> builder)
        {
            builder.HasNoKey();
            builder.ToView("TeamsWithMemeber");

        }

    }
}
