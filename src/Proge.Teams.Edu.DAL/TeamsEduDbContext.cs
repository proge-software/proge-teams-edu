using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Proge.Teams.Edu.DAL.Configurations;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Proge.Teams.Edu.DAL
{
    public class TeamsEduDbContext : DbContext
    {
        public TeamsEduDbContext(DbContextOptions options) : base(options)
        {

        }        

        public DbSet<Team> Teams { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);            

            modelBuilder.ApplyConfiguration(TeamConfiguration.Default);
            modelBuilder.ApplyConfiguration(MemberConfiguration.Default);
            modelBuilder.ApplyConfiguration(TeamMemberConfiguration.Default);


        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
#endif
            //if (!optionsBuilder.IsConfigured)
            //{
            //    IConfigurationRoot configuration = new ConfigurationBuilder()
            //       .SetBasePath(Directory.GetCurrentDirectory())
            //       .AddJsonFile("appsettings.json")
            //       .Build();
            //    var connectionString = configuration.GetConnectionString("DefaultConnection");
            //    optionsBuilder.UseSqlServer(connectionString);
            //}
        }

    }
}
