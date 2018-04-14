﻿// <auto-generated />
using System;
using Ferrous.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ferrous.Migrations
{
    [DbContext(typeof(ferrousContext))]
    partial class ferrousContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.0-preview2-30571");

            modelBuilder.Entity("Ferrous.Models.Building", b =>
                {
                    b.Property<string>("Location")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DefaultCapacity");

                    b.Property<string>("LocationFullName");

                    b.HasKey("Location");

                    b.ToTable("Building");
                });

            modelBuilder.Entity("Ferrous.Models.Contingent", b =>
                {
                    b.Property<string>("ContingentLeaderNo")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Remark");

                    b.HasKey("ContingentLeaderNo");

                    b.ToTable("Contingents");
                });

            modelBuilder.Entity("Ferrous.Models.ContingentArrival", b =>
                {
                    b.Property<int>("ContingentArrivalNo")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContingentLeaderNo");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<int?>("Female");

                    b.Property<int?>("FemaleOnSpot");

                    b.Property<int?>("Male");

                    b.Property<int?>("MaleOnSpot");

                    b.HasKey("ContingentArrivalNo");

                    b.HasIndex("ContingentLeaderNo");

                    b.ToTable("ContingentArrival");
                });

            modelBuilder.Entity("Ferrous.Models.Person", b =>
                {
                    b.Property<string>("Mino")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("College");

                    b.Property<string>("ContingentLeaderNo");

                    b.Property<string>("Email");

                    b.Property<string>("Name");

                    b.Property<string>("Phone");

                    b.Property<string>("Sex")
                        .HasMaxLength(1);

                    b.HasKey("Mino");

                    b.HasIndex("ContingentLeaderNo");

                    b.ToTable("Person");
                });

            modelBuilder.Entity("Ferrous.Models.Room", b =>
                {
                    b.Property<int>("RoomId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Capacity");

                    b.Property<string>("Location");

                    b.Property<string>("LocationExtra");

                    b.Property<string>("LockNo");

                    b.Property<string>("Remark");

                    b.Property<string>("RoomName");

                    b.Property<int?>("Status");

                    b.HasKey("RoomId");

                    b.HasIndex("Location");

                    b.ToTable("Room");
                });

            modelBuilder.Entity("Ferrous.Models.RoomAllocation", b =>
                {
                    b.Property<int>("Sno")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ContingentArrivalNo");

                    b.Property<string>("ContingentLeaderNo");

                    b.Property<int>("Partial");

                    b.Property<int>("RoomId");

                    b.HasKey("Sno");

                    b.HasIndex("ContingentArrivalNo");

                    b.HasIndex("ContingentLeaderNo");

                    b.HasIndex("RoomId");

                    b.ToTable("RoomAllocation");
                });

            modelBuilder.Entity("Ferrous.Models.ContingentArrival", b =>
                {
                    b.HasOne("Ferrous.Models.Contingent", "ContingentLeaderNoNavigation")
                        .WithMany("ContingentArrival")
                        .HasForeignKey("ContingentLeaderNo");
                });

            modelBuilder.Entity("Ferrous.Models.Person", b =>
                {
                    b.HasOne("Ferrous.Models.Contingent", "ContingentLeaderNoNavigation")
                        .WithMany("Person")
                        .HasForeignKey("ContingentLeaderNo");
                });

            modelBuilder.Entity("Ferrous.Models.Room", b =>
                {
                    b.HasOne("Ferrous.Models.Building", "LocationNavigation")
                        .WithMany("Room")
                        .HasForeignKey("Location");
                });

            modelBuilder.Entity("Ferrous.Models.RoomAllocation", b =>
                {
                    b.HasOne("Ferrous.Models.ContingentArrival", "ContingentArrivalNoNavigation")
                        .WithMany("RoomAllocation")
                        .HasForeignKey("ContingentArrivalNo");

                    b.HasOne("Ferrous.Models.Contingent", "ContingentLeaderNoNavigation")
                        .WithMany("RoomAllocation")
                        .HasForeignKey("ContingentLeaderNo");

                    b.HasOne("Ferrous.Models.Room", "Room")
                        .WithMany("RoomAllocation")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
