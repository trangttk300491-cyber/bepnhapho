using System.ComponentModel.DataAnnotations;
using BepNhaPho.Models;

namespace BepNhaPho.ViewModels;

public sealed class RecipeCardViewModel
{
    public required CongThuc Recipe { get; init; }
    public bool IsFavorite { get; init; }
}

public sealed class RecipeListViewModel
{
    public List<RecipeCardViewModel> Recipes { get; set; } = [];
    public List<DanhMuc> Categories { get; set; } = [];
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string Heading { get; set; } = "Công thức mới nhất";
}

public sealed class RecipeFormViewModel
{
    public int? MaCongThuc { get; set; }
    [Display(Name = "Tên công thức"), Required(ErrorMessage = "Vui lòng nhập tên công thức."), StringLength(160, ErrorMessage = "Tên công thức không được vượt quá {1} ký tự.")] public string TieuDe { get; set; } = "";
    [Display(Name = "Mô tả"), Required(ErrorMessage = "Vui lòng viết mô tả.")] public string MoTa { get; set; } = "";
    [Display(Name = "Danh mục"), Required(ErrorMessage = "Vui lòng chọn danh mục.")] public int? MaDanhMuc { get; set; }
    [Display(Name = "Thời gian nấu"), Range(1, 1440, ErrorMessage = "Thời gian nấu phải từ 1 đến 1440 phút.")] public int ThoiGianNauPhut { get; set; } = 30;
    [Display(Name = "Khẩu phần"), Range(1, 100, ErrorMessage = "Khẩu phần phải từ 1 đến 100.")] public int KhauPhan { get; set; } = 2;
    public IFormFile? AnhDaiDien { get; set; }
    public string? AnhHienTai { get; set; }
    public List<IngredientInput> NguyenLieus { get; set; } = [new()];
    public List<StepInput> BuocThucHiens { get; set; } = [new()];
    public List<DanhMuc> DanhMucs { get; set; } = [];
}

public sealed class IngredientInput
{
    [Display(Name = "Tên nguyên liệu"), Required(ErrorMessage = "Nhập tên nguyên liệu."), StringLength(120, ErrorMessage = "Tên nguyên liệu không được vượt quá {1} ký tự.")] public string TenNguyenLieu { get; set; } = "";
    [Display(Name = "Định lượng"), Required(ErrorMessage = "Nhập định lượng."), StringLength(100, ErrorMessage = "Định lượng không được vượt quá {1} ký tự.")] public string DinhLuong { get; set; } = "";
}

public sealed class StepInput
{
    [Display(Name = "Nội dung bước thực hiện"), Required(ErrorMessage = "Nhập nội dung bước làm.")] public string NoiDung { get; set; } = "";
}

public sealed class CategoryInputViewModel
{
    public int? MaDanhMuc { get; set; }
    [Display(Name = "Tên danh mục"), Required(ErrorMessage = "Vui lòng nhập tên danh mục."), StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá {1} ký tự.")] public string TenDanhMuc { get; set; } = "";
}

public sealed class AdminRecipeListViewModel
{
    public List<CongThuc> Recipes { get; set; } = [];
    public string? Status { get; set; }
}

public sealed class ReviewViewModel
{
    [Display(Name = "Mã công thức"), Required(ErrorMessage = "Vui lòng nhập mã công thức.")] public int MaCongThuc { get; set; }
    [Display(Name = "Lý do từ chối"), StringLength(300, MinimumLength = 5, ErrorMessage = "Lý do từ chối cần từ 5 đến 300 ký tự.")] public string? LyDoTuChoi { get; set; }
}
