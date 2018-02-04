using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ferrous.Models
{
    public partial class ferrousContext : DbContext
    {
        public virtual DbSet<Building> Building { get; set; }
        public virtual DbSet<ContingentArrival> ContingentArrival { get; set; }
        public virtual DbSet<Contingents> Contingents { get; set; }
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<RoomAllocation> RoomAllocation { get; set; }

        public ferrousContext() { }

        public ferrousContext(DbContextOptions<ferrousContext> options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(Startup.DatabaseConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasKey(e => e.Location);

                entity.HasIndex(e => e.Location)
                    .HasName("UQ__tmp_ms_x__E55D3B10386ED722")
                    .IsUnique();

                entity.Property(e => e.Location).ValueGeneratedNever();

                entity.Property(e => e.DefaultCapacity).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<ContingentArrival>(entity =>
            {
                entity.HasKey(e => e.ContingentArrivalNo);

                entity.HasIndex(e => e.ContingentLeaderNo);

                entity.Property(e => e.ContingentArrivalNo).ValueGeneratedNever();

                entity.Property(e => e.ContingentLeaderNo).IsRequired();

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Female).HasDefaultValueSql("((0))");

                entity.Property(e => e.FemaleOnSpot).HasDefaultValueSql("((0))");

                entity.Property(e => e.Male).HasDefaultValueSql("((0))");

                entity.Property(e => e.MaleOnSpot).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.ContingentLeaderNoNavigation)
                    .WithMany(p => p.ContingentArrival)
                    .HasForeignKey(d => d.ContingentLeaderNo)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Contingents>(entity =>
            {
                entity.HasKey(e => e.ContingentLeaderNo);

                entity.HasIndex(e => e.ContingentLeaderNo)
                    .HasName("UQ__tmp_ms_x__A652420B54B398B9")
                    .IsUnique();

                entity.Property(e => e.ContingentLeaderNo).ValueGeneratedNever();
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Mino);

                entity.HasIndex(e => e.ContingentLeaderNo);

                entity.Property(e => e.Mino)
                    .HasColumnName("MINo")
                    .ValueGeneratedNever();

                entity.Property(e => e.ContingentLeaderNo).IsRequired();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.ContingentLeaderNoNavigation)
                    .WithMany(p => p.Person)
                    .HasForeignKey(d => d.ContingentLeaderNo)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(e => e.Location);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Location).IsRequired();

                entity.Property(e => e.Room1)
                    .IsRequired()
                    .HasColumnName("Room");

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.LocationNavigation)
                    .WithMany(p => p.Room)
                    .HasForeignKey(d => d.Location)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<RoomAllocation>(entity =>
            {
                entity.HasKey(e => e.Sno);

                entity.HasIndex(e => e.ContingentArrivalNo);

                entity.HasIndex(e => e.ContingentLeaderNo);

                entity.HasIndex(e => e.RoomId);

                entity.Property(e => e.Sno)
                    .HasColumnName("SNo")
                    .ValueGeneratedNever();

                entity.Property(e => e.ContingentLeaderNo).IsRequired();

                entity.HasOne(d => d.ContingentArrivalNoNavigation)
                    .WithMany(p => p.RoomAllocation)
                    .HasForeignKey(d => d.ContingentArrivalNo);

                entity.HasOne(d => d.ContingentLeaderNoNavigation)
                    .WithMany(p => p.RoomAllocation)
                    .HasForeignKey(d => d.ContingentLeaderNo)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomAllocation)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
    }
}
