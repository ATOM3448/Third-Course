using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Lab4.Db;

public partial class LabsContext : DbContext
{
    public LabsContext()
    {
    }

    public LabsContext(DbContextOptions<LabsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientOrder> ClientOrders { get; set; }

    public virtual DbSet<Good> Goods { get; set; }

    public virtual DbSet<OrdGd> OrdGds { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=labs;Username=postgres;Password=002vTolnaS");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.CodeC).HasName("client_pkey");

            entity.ToTable("client");

            entity.Property(e => e.CodeC).HasColumnName("code_c");
            entity.Property(e => e.AddrC)
                .HasMaxLength(25)
                .HasColumnName("addr_c");
            entity.Property(e => e.Fio)
                .HasMaxLength(35)
                .HasColumnName("fio");
            entity.Property(e => e.Tel)
                .HasMaxLength(38)
                .HasColumnName("tel");
        });

        modelBuilder.Entity<ClientOrder>(entity =>
        {
            entity.HasKey(e => e.CodeZ).HasName("client_order_pkey");

            entity.ToTable("client_order");

            entity.Property(e => e.CodeZ)
                .HasMaxLength(9)
                .HasColumnName("code_z");
            entity.Property(e => e.AccDt).HasColumnName("acc_dt");
            entity.Property(e => e.AddrD)
                .HasMaxLength(25)
                .HasColumnName("addr_d");
            entity.Property(e => e.CodeC).HasColumnName("code_c");
            entity.Property(e => e.DelDt).HasColumnName("del_dt");
            entity.Property(e => e.PriceD)
                .HasColumnType("money")
                .HasColumnName("price_d");

            entity.HasOne(d => d.CodeCNavigation).WithMany(p => p.ClientOrders)
                .HasForeignKey(d => d.CodeC)
                .HasConstraintName("client_order_code_c_fkey");
        });

        modelBuilder.Entity<Good>(entity =>
        {
            entity.HasKey(e => e.Art).HasName("goods_pkey");

            entity.ToTable("goods");

            entity.Property(e => e.Art)
                .HasMaxLength(12)
                .IsFixedLength()
                .HasColumnName("art");
            entity.Property(e => e.Meas)
                .HasMaxLength(3)
                .HasColumnName("meas");
            entity.Property(e => e.NameG)
                .HasMaxLength(30)
                .HasColumnName("name_g");
            entity.Property(e => e.PriceG)
                .HasColumnType("money")
                .HasColumnName("price_g");
        });

        modelBuilder.Entity<OrdGd>(entity =>
        {
            entity.HasKey(e => new { e.CodeZ, e.Art }).HasName("ord_gd_pkey");

            entity.ToTable("ord_gd");

            entity.Property(e => e.CodeZ)
                .HasMaxLength(9)
                .HasColumnName("code_z");
            entity.Property(e => e.Art)
                .HasMaxLength(12)
                .HasColumnName("art");
            entity.Property(e => e.Qt).HasColumnName("qt");

            entity.HasOne(d => d.ArtNavigation).WithMany(p => p.OrdGds)
                .HasForeignKey(d => d.Art)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ord_gd_art_fkey");

            entity.HasOne(d => d.CodeZNavigation).WithMany(p => p.OrdGds)
                .HasForeignKey(d => d.CodeZ)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ord_gd_code_z_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
