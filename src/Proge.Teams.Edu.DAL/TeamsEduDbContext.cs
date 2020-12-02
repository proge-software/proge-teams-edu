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
        public DbSet<Log> Logs { get; set; }
        public DbSet<LatestLog> LatestLogs { get; set; }
        public DbSet<TeamWithMemeber> TeamsWithMemeber { get; set; }
        public DbSet<ChangeNotification> ChangeNotification { get; set; }
        public DbSet<CallRecord> CallRecord { get; set; }
        public DbSet<CallUser> CallUser { get; set; }
        public DbSet<CallSession> CallSession { get; set; }
        public DbSet<CallSegment> CallSessionSegment { get; set; }
        public DbSet<TeamsMeeting> TeamsMeeting { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);            

            modelBuilder.ApplyConfiguration(TeamConfiguration.Default);
            modelBuilder.ApplyConfiguration(MemberConfiguration.Default);
            modelBuilder.ApplyConfiguration(TeamMemberConfiguration.Default);
            modelBuilder.ApplyConfiguration(LogConfiguration.Default);
            modelBuilder.ApplyConfiguration(LatestLogConfiguration.Default);
            modelBuilder.ApplyConfiguration(TeamWithMemeberConfiguration.Default);
            modelBuilder.ApplyConfiguration(ChangeNotificationConfiguration.Default);
            modelBuilder.ApplyConfiguration(CallRecordConfiguration.Default);
            modelBuilder.ApplyConfiguration(CallUserConfiguration.Default);
            modelBuilder.ApplyConfiguration(CallSessionConfiguration.Default);
            modelBuilder.ApplyConfiguration(CallSegmentConfiguration.Default);
            modelBuilder.ApplyConfiguration(TeamsMeetingConfiguration.Default);
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
