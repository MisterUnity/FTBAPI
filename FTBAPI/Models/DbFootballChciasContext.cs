using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FTBAPI.Models;

public partial class DbFootballChciasContext : DbContext
{
    public DbFootballChciasContext()
    {
    }

    public DbFootballChciasContext(DbContextOptions<DbFootballChciasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Gameschedule> Gameschedules { get; set; }

    public virtual DbSet<Playergamesinfo> Playergamesinfos { get; set; }

    public virtual DbSet<Playerinfo> Playerinfos { get; set; }

    public virtual DbSet<UserAuthInfo> UserAuthInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Chinese_Taiwan_Stroke_CI_AS");

        modelBuilder.Entity<Gameschedule>(entity =>
        {
            entity.ToTable("gameschedule");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Field).HasMaxLength(10);
            entity.Property(e => e.Opponent).HasMaxLength(10);
        });

        modelBuilder.Entity<Playergamesinfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("playergamesinfo");

            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Id)
                .HasMaxLength(80)
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(10);
            entity.Property(e => e.Opponent).HasMaxLength(10);
            entity.Property(e => e.Place).HasMaxLength(10);
            entity.Property(e => e.Team).HasMaxLength(10);
        });

        modelBuilder.Entity<Playerinfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PLAYERDA__3214EC273FE26456");

            entity.ToTable("playerinfo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.Age)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(600);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Height).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(10);
            entity.Property(e => e.Photo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Position).HasMaxLength(10);
            entity.Property(e => e.Team).HasMaxLength(10);
            entity.Property(e => e.Weight).HasMaxLength(10);
        });

        modelBuilder.Entity<UserAuthInfo>(entity =>
        {
            entity.HasKey(e => e.Act);

            entity.ToTable("userAuthInfo", tb => tb.HasTrigger("SetExpiredAt"));

            entity.HasIndex(e => e.Act, "UQ__tmp_ms_x__DE50FEF7915889C4").IsUnique();

            entity.Property(e => e.Act)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("act");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Name)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Pwd)
                .IsUnicode(false)
                .HasColumnName("pwd");
            entity.Property(e => e.PwdExpired)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("pwdExpired");
            entity.Property(e => e.Usrlevl)
                .HasDefaultValueSql("((0))")
                .HasColumnName("usrlevl");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
