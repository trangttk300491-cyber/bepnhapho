using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BepNhaPho.Data;
using BepNhaPho.Models;
using BepNhaPho.ViewModels;

namespace BepNhaPho.Controllers;

public sealed class RecipeController(AppDbContext db, IWebHostEnvironment environment) : Controller
{
    private const int PageSize = 12;

    [AllowAnonymous]
    public Task<IActionResult> Index(string? q, int? categoryId, int page = 1) => ListAsync(q, categoryId, page);

    [AllowAnonymous]
    public Task<IActionResult> Search(string? q, int? categoryId, int page = 1) => ListAsync(q, categoryId, page);

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var recipe = await db.CongThuc.Include(x => x.DanhMuc).Include(x => x.NguoiDung)
            .Include(x => x.NguyenLieus).Include(x => x.BuocThucHiens)
            .SingleOrDefaultAsync(x => x.MaCongThuc == id);
        if (recipe is null) return NotFound();
        var owner = User.Identity?.IsAuthenticated == true && User.FindFirstValue(ClaimTypes.NameIdentifier) == recipe.MaNguoiDung.ToString();
        var admin = User.IsInRole("QuanTri");
        if (recipe.TrangThai != "DaDuyet" && !owner && !admin) return NotFound();
        ViewBag.IsFavorite = User.Identity?.IsAuthenticated == true && User.IsInRole("NguoiDung") &&
            await db.YeuThich.AnyAsync(x => x.MaNguoiDung == UserId() && x.MaCongThuc == id);
        ViewBag.IsOwner = owner;
        return View(recipe);
    }

    [Authorize(Roles = "NguoiDung")]
    public async Task<IActionResult> Create() => View("Form", await PrepareFormAsync(new RecipeFormViewModel()));

    [HttpPost, Authorize(Roles = "NguoiDung"), ValidateAntiForgeryToken, RequestSizeLimit(3_000_000)]
    public async Task<IActionResult> Create(RecipeFormViewModel model)
    {
        if (model.AnhDaiDien is null) ModelState.AddModelError(nameof(model.AnhDaiDien), "Vui lòng chọn ảnh đại diện.");
        if (model.MaDanhMuc.HasValue && !await db.DanhMuc.AnyAsync(x => x.MaDanhMuc == model.MaDanhMuc.Value))
            ModelState.AddModelError(nameof(model.MaDanhMuc), "Danh mục đã chọn không tồn tại.");
        if (!ValidateRecipeLines(model)) return View("Form", await PrepareFormAsync(model));
        if (!ModelState.IsValid) return View("Form", await PrepareFormAsync(model));

        string? storedImage = null;
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            storedImage = await SaveImageAsync(model.AnhDaiDien!);
            var recipe = BuildRecipe(model, storedImage!, UserId());
            db.CongThuc.Add(recipe);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = "Công thức đang chờ duyệt.";
            return RedirectToAction(nameof(Mine));
        }
        catch (InvalidImageException ex)
        {
            ModelState.AddModelError(nameof(model.AnhDaiDien), ex.Message);
            return View("Form", await PrepareFormAsync(model));
        }
        catch
        {
            if (storedImage is not null) DeleteImage(storedImage);
            ModelState.AddModelError(string.Empty, "Chưa thể lưu công thức. Vui lòng thử lại.");
            return View("Form", await PrepareFormAsync(model));
        }
    }

    [Authorize(Roles = "NguoiDung")]
    public async Task<IActionResult> Edit(int id)
    {
        var recipe = await db.CongThuc.Include(x => x.NguyenLieus).Include(x => x.BuocThucHiens)
            .SingleOrDefaultAsync(x => x.MaCongThuc == id && x.MaNguoiDung == UserId());
        if (recipe is null) return NotFound();
        var model = new RecipeFormViewModel
        {
            MaCongThuc = id, TieuDe = recipe.TieuDe, MoTa = recipe.MoTa, MaDanhMuc = recipe.MaDanhMuc,
            ThoiGianNauPhut = recipe.ThoiGianNauPhut, KhauPhan = recipe.KhauPhan, AnhHienTai = recipe.AnhDaiDien,
            NguyenLieus = recipe.NguyenLieus.OrderBy(x => x.MaNguyenLieu).Select(x => new IngredientInput { TenNguyenLieu = x.TenNguyenLieu, DinhLuong = x.DinhLuong }).ToList(),
            BuocThucHiens = recipe.BuocThucHiens.OrderBy(x => x.SoThuTu).Select(x => new StepInput { NoiDung = x.NoiDung }).ToList()
        };
        return View("Form", await PrepareFormAsync(model));
    }

    [HttpPost, Authorize(Roles = "NguoiDung"), ValidateAntiForgeryToken, RequestSizeLimit(3_000_000)]
    public async Task<IActionResult> Edit(int id, RecipeFormViewModel model)
    {
        if (model.MaCongThuc != id) return BadRequest();
        var recipe = await db.CongThuc.Include(x => x.NguyenLieus).Include(x => x.BuocThucHiens)
            .SingleOrDefaultAsync(x => x.MaCongThuc == id && x.MaNguoiDung == UserId());
        if (recipe is null) return NotFound();
        model.AnhHienTai = recipe.AnhDaiDien;
        if (model.MaDanhMuc.HasValue && !await db.DanhMuc.AnyAsync(x => x.MaDanhMuc == model.MaDanhMuc.Value))
            ModelState.AddModelError(nameof(model.MaDanhMuc), "Danh mục đã chọn không tồn tại.");
        ValidateRecipeLines(model);
        if (!ModelState.IsValid) return View("Form", await PrepareFormAsync(model));

        string? newImage = null;
        try
        {
            if (model.AnhDaiDien is not null) newImage = await SaveImageAsync(model.AnhDaiDien);
            recipe.TieuDe = model.TieuDe.Trim();
            recipe.MoTa = model.MoTa.Trim();
            recipe.MaDanhMuc = model.MaDanhMuc!.Value;
            recipe.ThoiGianNauPhut = model.ThoiGianNauPhut;
            recipe.KhauPhan = model.KhauPhan;
            if (newImage is not null) recipe.AnhDaiDien = newImage;
            recipe.TrangThai = "ChoDuyet";
            recipe.LyDoTuChoi = null;
            db.NguyenLieu.RemoveRange(recipe.NguyenLieus);
            db.BuocThucHien.RemoveRange(recipe.BuocThucHiens);
            recipe.NguyenLieus = BuildIngredients(model);
            recipe.BuocThucHiens = BuildSteps(model);
            await db.SaveChangesAsync();
            if (newImage is not null) DeleteImage(model.AnhHienTai!);
            TempData["Success"] = "Công thức đã được cập nhật và chuyển về trạng thái chờ duyệt.";
            return RedirectToAction(nameof(Mine));
        }
        catch (InvalidImageException ex)
        {
            ModelState.AddModelError(nameof(model.AnhDaiDien), ex.Message);
            return View("Form", await PrepareFormAsync(model));
        }
        catch
        {
            if (newImage is not null) DeleteImage(newImage);
            ModelState.AddModelError(string.Empty, "Chưa thể lưu công thức. Vui lòng thử lại.");
            return View("Form", await PrepareFormAsync(model));
        }
    }

    [HttpPost, Authorize(Roles = "NguoiDung"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var recipe = await db.CongThuc.SingleOrDefaultAsync(x => x.MaCongThuc == id && x.MaNguoiDung == UserId());
        if (recipe is null) return NotFound();
        recipe.DaXoa = true;
        await db.SaveChangesAsync();
        TempData["Success"] = "Công thức đã được xóa.";
        return RedirectToAction(nameof(Mine));
    }

    [Authorize(Roles = "NguoiDung")]
    public async Task<IActionResult> Mine()
    {
        var recipes = await db.CongThuc.Include(x => x.DanhMuc).Where(x => x.MaNguoiDung == UserId())
            .OrderByDescending(x => x.NgayTao).ToListAsync();
        return View(recipes);
    }

    [Authorize(Roles = "NguoiDung")]
    public async Task<IActionResult> Favorites()
    {
        var favorites = await db.YeuThich.AsNoTracking()
            .Where(x => x.MaNguoiDung == UserId() && x.CongThuc.TrangThai == "DaDuyet")
            .Include(x => x.CongThuc).ThenInclude(x => x.DanhMuc)
            .OrderByDescending(x => x.NgayTao)
            .ToListAsync();
        return View(favorites.Select(x => x.CongThuc).ToList());
    }

    private async Task<IActionResult> ListAsync(string? q, int? categoryId, int page)
    {
        page = Math.Max(page, 1);
        var recipes = db.CongThuc.AsNoTracking().Where(x => x.TrangThai == "DaDuyet");
        if (categoryId.HasValue) recipes = recipes.Where(x => x.MaDanhMuc == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            recipes = recipes.Where(x => x.TieuDe.Contains(term) || x.NguyenLieus.Any(n => n.TenNguyenLieu.Contains(term)));
        }
        var total = await recipes.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)PageSize);
        page = Math.Min(page, Math.Max(1, totalPages));
        var rows = await recipes.Include(x => x.DanhMuc).OrderByDescending(x => x.NgayTao)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
        var favoriteIds = User.Identity?.IsAuthenticated == true && User.IsInRole("NguoiDung")
            ? await db.YeuThich.Where(x => x.MaNguoiDung == UserId() && rows.Select(r => r.MaCongThuc).Contains(x.MaCongThuc)).Select(x => x.MaCongThuc).ToListAsync()
            : [];
        return View("Index", new RecipeListViewModel
        {
            Query = q,
            CategoryId = categoryId,
            Page = page,
            TotalPages = totalPages,
            Categories = await db.DanhMuc.AsNoTracking().OrderBy(x => x.TenDanhMuc).ToListAsync(),
            Recipes = rows.Select(r => new RecipeCardViewModel { Recipe = r, IsFavorite = favoriteIds.Contains(r.MaCongThuc) }).ToList(),
            Heading = string.IsNullOrWhiteSpace(q) ? "Công thức mới nhất" : $"Kết quả cho “{q.Trim()}”"
        });
    }

    private async Task<RecipeFormViewModel> PrepareFormAsync(RecipeFormViewModel model)
    {
        model.DanhMucs = await db.DanhMuc.AsNoTracking().OrderBy(x => x.TenDanhMuc).ToListAsync();
        if (model.NguyenLieus.Count == 0) model.NguyenLieus = [new()];
        if (model.BuocThucHiens.Count == 0) model.BuocThucHiens = [new()];
        return model;
    }

    private bool ValidateRecipeLines(RecipeFormViewModel model)
    {
        if (model.NguyenLieus.Count == 0) ModelState.AddModelError(nameof(model.NguyenLieus), "Thêm ít nhất một nguyên liệu.");
        if (model.BuocThucHiens.Count == 0) ModelState.AddModelError(nameof(model.BuocThucHiens), "Thêm ít nhất một bước thực hiện.");
        return ModelState.IsValid;
    }

    private static CongThuc BuildRecipe(RecipeFormViewModel model, string image, int userId) => new()
    {
        TieuDe = model.TieuDe.Trim(), MoTa = model.MoTa.Trim(), AnhDaiDien = image,
        MaDanhMuc = model.MaDanhMuc!.Value, MaNguoiDung = userId,
        ThoiGianNauPhut = model.ThoiGianNauPhut, KhauPhan = model.KhauPhan,
        TrangThai = "ChoDuyet", NgayTao = DateTime.UtcNow,
        NguyenLieus = BuildIngredients(model), BuocThucHiens = BuildSteps(model)
    };

    private static List<NguyenLieu> BuildIngredients(RecipeFormViewModel model) => model.NguyenLieus
        .Where(x => !string.IsNullOrWhiteSpace(x.TenNguyenLieu) || !string.IsNullOrWhiteSpace(x.DinhLuong))
        .Select(x => new NguyenLieu { TenNguyenLieu = x.TenNguyenLieu.Trim(), DinhLuong = x.DinhLuong.Trim() }).ToList();

    private static List<BuocThucHien> BuildSteps(RecipeFormViewModel model) => model.BuocThucHiens
        .Where(x => !string.IsNullOrWhiteSpace(x.NoiDung))
        .Select((x, i) => new BuocThucHien { SoThuTu = i + 1, NoiDung = x.NoiDung.Trim() }).ToList();

    private int UserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<string> SaveImageAsync(IFormFile image)
    {
        if (image.Length == 0 || image.Length > 2 * 1024 * 1024) throw new InvalidImageException("Ảnh phải có dung lượng tối đa 2 MB.");
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png")) throw new InvalidImageException("Chỉ nhận ảnh JPG hoặc PNG.");
        var header = new byte[12];
        await using (var stream = image.OpenReadStream())
        {
            var read = await stream.ReadAtLeastAsync(header.AsMemory(), 12, throwOnEndOfStream: false);
            var valid = extension switch
            {
                ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => read >= 8 && header.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
                _ => false
            };
            if (!valid) throw new InvalidImageException("Tệp tải lên không đúng định dạng ảnh đã chọn.");
        }
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "recipes");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        await using var output = System.IO.File.Create(Path.Combine(folder, name));
        await image.CopyToAsync(output);
        return $"/uploads/recipes/{name}";
    }

    private void DeleteImage(string imagePath)
    {
        if (!imagePath.StartsWith("/uploads/recipes/", StringComparison.Ordinal)) return;
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var file = Path.Combine(webRoot, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
    }

    private sealed class InvalidImageException(string message) : Exception(message);
}
