using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ferrous.Models
{
    public partial class mydbContext : DbContext
    {
        public virtual DbSet<Building> Building { get; set; }
        public virtual DbSet<Contingents> Contingents { get; set; }
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<RoomAllocation> RoomAllocation { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=mydb;Trusted_Connection=True;");
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

                entity.Property(e => e.Location)
                    .HasMaxLength(15)
                    .ValueGeneratedNever();

                entity.Property(e => e.DefaultCapacity).HasDefaultValueSql("((0))");

                entity.Property(e => e.LocationFullName).HasMaxLength(50);
            });

            modelBuilder.Entity<Contingents>(entity =>
            {
                entity.HasKey(e => e.ContingentLeaderNo);

                entity.HasIndex(e => e.ContingentLeaderNo)
                    .HasName("UQ__tmp_ms_x__A652420B54B398B9")
                    .IsUnique();

                entity.Property(e => e.ContingentLeaderNo)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.AllocatedRooms)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ArrivedF).HasDefaultValueSql("((0))");

                entity.Property(e => e.ArrivedM).HasDefaultValueSql("((0))");

                entity.Property(e => e.Female).HasDefaultValueSql("((0))");

                entity.Property(e => e.Male).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Mino);

                entity.Property(e => e.Mino)
                    .HasColumnName("MINo")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.ContingentLeaderNo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.Sex).HasColumnType("char(1)");

                entity.HasOne(d => d.ContingentLeaderNoNavigation)
                    .WithMany(p => p.Person)
                    .HasForeignKey(d => d.ContingentLeaderNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__mydatatab__Conti__403A8C7D");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(e => e.Allocated).HasDefaultValueSql("((0))");

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.LocationExtra).HasMaxLength(15);

                entity.Property(e => e.LockNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Room1)
                    .IsRequired()
                    .HasColumnName("Room")
                    .HasMaxLength(10);

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.LocationNavigation)
                    .WithMany(p => p.Room)
                    .HasForeignKey(d => d.Location)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Room__Location__04E4BC85");
            });

            modelBuilder.Entity<RoomAllocation>(entity =>
            {
                entity.HasKey(e => e.Sno);

                entity.Property(e => e.Sno).HasColumnName("SNo");

                entity.Property(e => e.ContingentLeaderNo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.ContingentLeaderNoNavigation)
                    .WithMany(p => p.RoomAllocation)
                    .HasForeignKey(d => d.ContingentLeaderNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoomAllocation_Contingents");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomAllocation)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoomAllocation_Room");
            });
        }
    }
}
