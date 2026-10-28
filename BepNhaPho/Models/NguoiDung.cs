using System.ComponentModel.DataAnnotations;

namespace BepNhaPho.Models;

public sealed class NguoiDung
{
    public int MaNguoiDung { get; set; }
    [Display(Name = "Họ và tên"), Required(ErrorMessage = "Vui lòng nhập họ và tên."), StringLength(100, ErrorMessage = "Họ và tên không được vượt quá {1} ký tự.")] public string HoTen { get; set; } = "";
    [Display(Name = "Email"), Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), StringLength(256, ErrorMessage = "Email không được vượt quá {1} ký tự.")] public string Email { get; set; } = "";
    [Display(Name = "Mật khẩu đã mã hóa"), Required(ErrorMessage = "Vui lòng nhập mật khẩu đã mã hóa."), StringLength(512, ErrorMessage = "Mật khẩu đã mã hóa không được vượt quá {1} ký tự.")] public string MatKhauMaHoa { get; set; } = "";
    [Display(Name = "Vai trò"), Required(ErrorMessage = "Vui lòng nhập vai trò."), StringLength(20, ErrorMessage = "Vai trò không được vượt quá {1} ký tự.")] public string VaiTro { get; set; } = "NguoiDung";
    public bool HoatDong { get; set; } = true;
    [Display(Name = "Mã đặt lại mật khẩu"), StringLength(128, ErrorMessage = "Mã đặt lại mật khẩu không được vượt quá {1} ký tự.")] public string? MaDatLaiMatKhau { get; set; }
    public DateTime? HanMaDatLai { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public ICollection<CongThuc> CongThucs { get; set; } = new List<CongThuc>();
    public ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
}
