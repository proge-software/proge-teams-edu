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
    [Migration("20200909212456_AddedTeamMemberKey")]
    partial class AddedTeamMemberKey
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Proge.Teams.Edu.UniMoRe.BusinessLogic.DAL.Entities.Member", b =>
                {
                    b.Property<Guid>("Id")
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

                    b.HasKey("Id");

                    b.HasIndex("UserPrincipalName");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("Proge.Teams.Edu.UniMoRe.BusinessLogic.DAL.Entities.Team", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

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

                    b.Property<Guid?>("TeamsId")
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(30);

                    b.Property<Guid?>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ExternalId");

                    b.HasIndex("TeamsId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("Proge.Teams.Edu.UniMoRe.BusinessLogic.DAL.Entities.TeamMember", b =>
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

            modelBuilder.Entity("Proge.Teams.Edu.UniMoRe.BusinessLogic.DAL.Entities.TeamMember", b =>
                {
                    b.HasOne("Proge.Teams.Edu.UniMoRe.BusinessLogic.DAL.Entities.Member", "Member")
                        .WithMany("TeamsUsers")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Proge.Teams.Edu.UniMoRe.BusinessLogic.DAL.Entities.Team", "Team")
                        .WithMany("TeamsUsers")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
