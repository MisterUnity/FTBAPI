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

    public virtual DbSet<Playerinfo> Playerinfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:unity-yes.database.windows.net,1433;Initial Catalog=DB_FootballCHCIAS;Persist Security Info=False;User ID=weichehuang;Password=XGC0EW20GS00U5H1_@aq;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Chinese_Taiwan_Stroke_CI_AS");

        modelBuilder.Entity<Playerinfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PLAYERDA__3214EC273FE26456");

            entity.ToTable("playerinfo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.Brithday)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Description).HasMaxLength(600);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Height).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(10);
            entity.Property(e => e.NextPosition).HasMaxLength(10);
            entity.Property(e => e.Photo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Position).HasMaxLength(10);
            entity.Property(e => e.Weight).HasMaxLength(10);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
