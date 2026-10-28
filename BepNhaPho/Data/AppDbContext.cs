using Microsoft.EntityFrameworkCore;
using BepNhaPho.Models;

namespace BepNhaPho.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();
    public DbSet<DanhMuc> DanhMuc => Set<DanhMuc>();
    public DbSet<CongThuc> CongThuc => Set<CongThuc>();
    public DbSet<NguyenLieu> NguyenLieu => Set<NguyenLieu>();
    public DbSet<BuocThucHien> BuocThucHien => Set<BuocThucHien>();
    public DbSet<YeuThich> YeuThich => Set<YeuThich>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.ToTable("NguoiDung", t => t.HasCheckConstraint("CK_NguoiDung_VaiTro", "[VaiTro] IN (N'QuanTri', N'NguoiDung')"));
            entity.HasKey(x => x.MaNguoiDung);
            entity.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.MatKhauMaHoa).HasMaxLength(512).IsRequired();
            entity.Property(x => x.VaiTro).HasMaxLength(20).IsRequired();
            entity.Property(x => x.MaDatLaiMatKhau).HasMaxLength(128);
            entity.Property(x => x.HanMaDatLai).HasColumnType("datetime");
            entity.Property(x => x.NgayTao).HasColumnType("datetime");
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.ToTable("DanhMuc");
            entity.HasKey(x => x.MaDanhMuc);
            entity.Property(x => x.TenDanhMuc).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DaXoa).HasDefaultValue(false);
            entity.HasQueryFilter(x => !x.DaXoa);
            entity.HasIndex(x => x.TenDanhMuc).IsUnique();
        });

        modelBuilder.Entity<CongThuc>(entity =>
        {
            entity.ToTable("CongThuc", t =>
            {
                t.HasCheckConstraint("CK_CongThuc_LyDoTuChoi", "[TrangThai] <> N'BiTuChoi' OR ([LyDoTuChoi] IS NOT NULL AND LEN([LyDoTuChoi]) >= 5)");
                t.HasCheckConstraint("CK_CongThuc_TrangThai", "[TrangThai] IN (N'ChoDuyet', N'DaDuyet', N'BiTuChoi')");
            });
            entity.HasKey(x => x.MaCongThuc);
            entity.Property(x => x.TieuDe).HasMaxLength(160).IsRequired();
            entity.Property(x => x.MoTa).IsRequired();
            entity.Property(x => x.AnhDaiDien).HasMaxLength(260).IsRequired();
            entity.Property(x => x.TrangThai).HasMaxLength(20).IsRequired();
            entity.Property(x => x.LyDoTuChoi).HasMaxLength(300);
            entity.Property(x => x.NgayTao).HasColumnType("datetime");
            entity.Property(x => x.DaXoa).HasDefaultValue(false);
            entity.HasQueryFilter(x => !x.DaXoa);
            entity.HasOne(x => x.DanhMuc).WithMany(x => x.CongThucs).HasForeignKey(x => x.MaDanhMuc).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.NguoiDung).WithMany(x => x.CongThucs).HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NguyenLieu>(entity =>
        {
            entity.ToTable("NguyenLieu");
            entity.HasKey(x => x.MaNguyenLieu);
            entity.Property(x => x.TenNguyenLieu).HasMaxLength(120).IsRequired();
            entity.Property(x => x.DinhLuong).HasMaxLength(100).IsRequired();
            entity.HasQueryFilter(x => !x.CongThuc.DaXoa);
            entity.HasOne(x => x.CongThuc).WithMany(x => x.NguyenLieus).HasForeignKey(x => x.MaCongThuc).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BuocThucHien>(entity =>
        {
            entity.ToTable("BuocThucHien");
            entity.HasKey(x => x.MaBuoc);
            entity.Property(x => x.NoiDung).IsRequired();
            entity.HasIndex(x => new { x.MaCongThuc, x.SoThuTu }).IsUnique();
            entity.HasQueryFilter(x => !x.CongThuc.DaXoa);
            entity.HasOne(x => x.CongThuc).WithMany(x => x.BuocThucHiens).HasForeignKey(x => x.MaCongThuc).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<YeuThich>(entity =>
        {
            entity.ToTable("YeuThich");
            entity.HasKey(x => x.MaYeuThich);
            entity.Property(x => x.NgayTao).HasColumnType("datetime");
            entity.HasIndex(x => new { x.MaCongThuc, x.MaNguoiDung }).IsUnique();
            entity.HasQueryFilter(x => !x.CongThuc.DaXoa);
            entity.HasOne(x => x.CongThuc).WithMany(x => x.YeuThichs).HasForeignKey(x => x.MaCongThuc).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.NguoiDung).WithMany(x => x.YeuThichs).HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
