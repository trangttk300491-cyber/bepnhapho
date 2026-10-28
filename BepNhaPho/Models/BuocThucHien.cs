using System.ComponentModel.DataAnnotations;

namespace BepNhaPho.Models;

public sealed class BuocThucHien
{
    public int MaBuoc { get; set; }
    public int MaCongThuc { get; set; }
    public int SoThuTu { get; set; }
    [Display(Name = "Nội dung bước thực hiện"), Required(ErrorMessage = "Vui lòng nhập nội dung bước thực hiện.")] public string NoiDung { get; set; } = "";
    public CongThuc CongThuc { get; set; } = null!;
}
