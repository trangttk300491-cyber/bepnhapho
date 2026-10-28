namespace BepNhaPho.Models;

public sealed class YeuThich
{
    public int MaYeuThich { get; set; }
    public int MaCongThuc { get; set; }
    public int MaNguoiDung { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public CongThuc CongThuc { get; set; } = null!;
    public NguoiDung NguoiDung { get; set; } = null!;
}
