﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Proge.Teams.Edu.DAL;

namespace Proge.Teams.Edu.DAL.Migrations
{
    [DbContext(typeof(TeamsEduDbContext))]
    [Migration("20201104092250_TeamsMeetingTable")]
    partial class TeamsMeetingTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.CallRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CallDescription")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset?>("EndDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("JoinWebUrl")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Modalities")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset?>("StartDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("JoinWebUrl");

                    b.HasIndex("Type");

                    b.ToTable("CallRecord");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.CallSegment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CallSessionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CallUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CallUserRole")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTimeOffset?>("EndDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("StartDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("UserPlatform")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("UserProductFamily")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("CallSessionId");

                    b.HasIndex("Id");

                    b.ToTable("CallSessionSegment");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.CallSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CallRecordId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CallUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CallUserRole")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTimeOffset?>("EndDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("StartDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("UserPlatform")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("UserProductFamily")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("CallRecordId");

                    b.HasIndex("Id");

                    b.ToTable("CallSession");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.CallUser", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UserRole")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<Guid>("CallRecordId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(400)")
                        .HasMaxLength(400);

                    b.Property<Guid?>("UserTenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id", "UserRole", "CallRecordId");

                    b.HasIndex("CallRecordId");

                    b.HasIndex("DisplayName");

                    b.HasIndex("UserRole");

                    b.ToTable("CallUser");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.ChangeNotification", b =>
                {
                    b.Property<Guid?>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ODataId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ChangeType")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("ODataType")
                        .HasColumnType("nvarchar(400)")
                        .HasMaxLength(400);

                    b.Property<string>("RawJson")
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(2147483647);

                    b.Property<string>("Resource")
                        .HasColumnType("nvarchar(400)")
                        .HasMaxLength(400);

                    b.Property<DateTimeOffset?>("SubscriptionExpirationDateTime")
                        .HasColumnType("datetimeoffset(7)");

                    b.Property<Guid?>("SubscriptionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id", "ODataId", "ChangeType");

                    b.HasIndex("ChangeType");

                    b.ToTable("ChangeNotification");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Exception")
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(2147483647);

                    b.Property<string>("Level")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("LogEvent")
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(2147483647);

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(2147483647);

                    b.Property<string>("MessageTemplate")
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(2147483647);

                    b.Property<string>("Properties")
                        .HasColumnType("xml");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("datetimeoffset(7)");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("Level");

                    b.HasIndex("TimeStamp");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.Member", b =>
                {
                    b.Property<Guid>("MemberId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(400)")
                        .HasMaxLength(400);

                    b.Property<string>("JobTitle")
                        .HasColumnType("nvarchar(300)")
                        .HasMaxLength(300);

                    b.Property<string>("Mail")
                        .HasColumnType("nvarchar(300)")
                        .HasMaxLength(300);

                    b.Property<string>("OfficeLocation")
                        .HasColumnType("nvarchar(300)")
                        .HasMaxLength(300);

                    b.Property<Guid?>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UserPrincipalName")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("MemberId");

                    b.HasIndex("UserPrincipalName");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.Team", b =>
                {
                    b.Property<Guid>("TeamsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(30);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(1000)")
                        .HasMaxLength(1000);

                    b.Property<string>("ExternalId")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("InternalId")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<bool>("IsMembershipLimitedToOwners")
                        .HasColumnType("bit");

                    b.Property<string>("JoinCode")
                        .HasColumnType("nvarchar(30)")
                        .HasMaxLength(30);

                    b.Property<string>("JoinUrl")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(1000)")
                        .HasMaxLength(1000);

                    b.Property<string>("TeamType")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<Guid?>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TeamsId");

                    b.HasIndex("ExternalId");

                    b.HasIndex("TeamType");

                    b.HasIndex("TeamsId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.TeamMember", b =>
                {
                    b.Property<Guid>("TeamId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MemberType")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("TeamId", "MemberId", "MemberType");

                    b.HasIndex("MemberId");

                    b.HasIndex("MemberType");

                    b.ToTable("TeamMembers");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.TeamsMeeting", b =>
                {
                    b.Property<string>("JoinUrl")
                        .HasColumnType("nvarchar(450)")
                        .HasMaxLength(450);

                    b.Property<string>("MeetingId")
                        .HasColumnType("nvarchar(150)")
                        .HasMaxLength(150);

                    b.Property<string>("MeetingName")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("JoinUrl");

                    b.ToTable("TeamsMeeting");
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.CallSegment", b =>
                {
                    b.HasOne("Proge.Teams.Edu.DAL.Entities.CallSession", "CallSession")
                        .WithMany("CallSegments")
                        .HasForeignKey("CallSessionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.CallSession", b =>
                {
                    b.HasOne("Proge.Teams.Edu.DAL.Entities.CallRecord", "CallRecord")
                        .WithMany("CallSessions")
                        .HasForeignKey("CallRecordId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.CallUser", b =>
                {
                    b.HasOne("Proge.Teams.Edu.DAL.Entities.CallRecord", "CallRecord")
                        .WithMany("CallUsers")
                        .HasForeignKey("CallRecordId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Proge.Teams.Edu.DAL.Entities.TeamMember", b =>
                {
                    b.HasOne("Proge.Teams.Edu.DAL.Entities.Member", "Member")
                        .WithMany("TeamsUsers")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Proge.Teams.Edu.DAL.Entities.Team", "Team")
                        .WithMany("TeamsUsers")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
