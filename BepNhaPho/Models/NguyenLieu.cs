using System.ComponentModel.DataAnnotations;

namespace BepNhaPho.Models;

public sealed class NguyenLieu
{
    public int MaNguyenLieu { get; set; }
    public int MaCongThuc { get; set; }
    [Display(Name = "Tên nguyên liệu"), Required(ErrorMessage = "Vui lòng nhập tên nguyên liệu."), StringLength(120, ErrorMessage = "Tên nguyên liệu không được vượt quá {1} ký tự.")] public string TenNguyenLieu { get; set; } = "";
    [Display(Name = "Định lượng"), Required(ErrorMessage = "Vui lòng nhập định lượng."), StringLength(100, ErrorMessage = "Định lượng không được vượt quá {1} ký tự.")] public string DinhLuong { get; set; } = "";
    public CongThuc CongThuc { get; set; } = null!;
}
