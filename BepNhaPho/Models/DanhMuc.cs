using System.ComponentModel.DataAnnotations;

namespace BepNhaPho.Models;

public sealed class DanhMuc
{
    public int MaDanhMuc { get; set; }
    [Display(Name = "Tên danh mục"), Required(ErrorMessage = "Vui lòng nhập tên danh mục."), StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá {1} ký tự.")] public string TenDanhMuc { get; set; } = "";
    public bool DaXoa { get; set; }
    public ICollection<CongThuc> CongThucs { get; set; } = new List<CongThuc>();
}
