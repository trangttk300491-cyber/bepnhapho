using System.ComponentModel.DataAnnotations;

namespace BepNhaPho.Models;

public sealed class CongThuc
{
    public int MaCongThuc { get; set; }
    [Display(Name = "Tên công thức"), Required(ErrorMessage = "Vui lòng nhập tên công thức."), StringLength(160, ErrorMessage = "Tên công thức không được vượt quá {1} ký tự.")] public string TieuDe { get; set; } = "";
    [Display(Name = "Mô tả"), Required(ErrorMessage = "Vui lòng nhập mô tả.")] public string MoTa { get; set; } = "";
    [Display(Name = "Ảnh đại diện"), Required(ErrorMessage = "Vui lòng nhập ảnh đại diện."), StringLength(260, ErrorMessage = "Ảnh đại diện không được vượt quá {1} ký tự.")] public string AnhDaiDien { get; set; } = "";
    public int MaDanhMuc { get; set; }
    public int MaNguoiDung { get; set; }
    [Display(Name = "Thời gian nấu"), Range(1, 1440, ErrorMessage = "Thời gian nấu phải từ {1} đến {2}.")] public int ThoiGianNauPhut { get; set; }
    [Display(Name = "Khẩu phần"), Range(1, 100, ErrorMessage = "Khẩu phần phải từ {1} đến {2}.")] public int KhauPhan { get; set; }
    [Display(Name = "Trạng thái"), Required(ErrorMessage = "Vui lòng nhập trạng thái."), StringLength(20, ErrorMessage = "Trạng thái không được vượt quá {1} ký tự.")] public string TrangThai { get; set; } = "ChoDuyet";
    [Display(Name = "Lý do từ chối"), StringLength(300, ErrorMessage = "Lý do từ chối không được vượt quá {1} ký tự.")] public string? LyDoTuChoi { get; set; }
    public bool DaXoa { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DanhMuc DanhMuc { get; set; } = null!;
    public NguoiDung NguoiDung { get; set; } = null!;
    public ICollection<NguyenLieu> NguyenLieus { get; set; } = new List<NguyenLieu>();
    public ICollection<BuocThucHien> BuocThucHiens { get; set; } = new List<BuocThucHien>();
    public ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
}
