using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BepNhaPho.Data;
using BepNhaPho.Models;
using BepNhaPho.ViewModels;

namespace BepNhaPho.Controllers;

[Authorize(Roles = "QuanTri")]
public sealed class AdminController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Statistics()
    {
        var recipeCounts = await db.CongThuc.AsNoTracking()
            .GroupBy(x => x.TrangThai)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
        var userCounts = await db.NguoiDung.AsNoTracking()
            .GroupBy(x => x.HoatDong)
            .Select(group => new { Active = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Active, x => x.Count);
        var categories = await db.DanhMuc.AsNoTracking()
            .OrderBy(x => x.TenDanhMuc)
            .Select(category => new CategoryStatisticsViewModel
            {
                Name = category.TenDanhMuc,
                PendingRecipes = category.CongThucs.Count(x => x.TrangThai == "ChoDuyet"),
                ApprovedRecipes = category.CongThucs.Count(x => x.TrangThai == "DaDuyet"),
                RejectedRecipes = category.CongThucs.Count(x => x.TrangThai == "BiTuChoi")
            }).ToListAsync();

        return View(new AdminStatisticsViewModel
        {
            PendingRecipes = recipeCounts.GetValueOrDefault("ChoDuyet"),
            ApprovedRecipes = recipeCounts.GetValueOrDefault("DaDuyet"),
            RejectedRecipes = recipeCounts.GetValueOrDefault("BiTuChoi"),
            ActiveUsers = userCounts.GetValueOrDefault(true),
            LockedUsers = userCounts.GetValueOrDefault(false),
            Categories = categories
        });
    }

    public async Task<IActionResult> Pending()
    {
        var recipes = await db.CongThuc.Include(x => x.DanhMuc).Include(x => x.NguoiDung)
            .Include(x => x.NguyenLieus).Include(x => x.BuocThucHiens)
            .Where(x => x.TrangThai == "ChoDuyet").OrderBy(x => x.NgayTao).ToListAsync();
        return View(recipes);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewViewModel model, string decision)
    {
        var recipe = await db.CongThuc.SingleOrDefaultAsync(x => x.MaCongThuc == model.MaCongThuc);
        if (recipe is null) return NotFound();
        if (recipe.TrangThai != "ChoDuyet")
        {
            TempData["Error"] = "Công thức này đã được xử lý.";
            return RedirectToAction(nameof(Pending));
        }
        if (decision == "approve")
        {
            recipe.TrangThai = "DaDuyet";
            recipe.LyDoTuChoi = null;
        }
        else if (decision == "reject")
        {
            if (string.IsNullOrWhiteSpace(model.LyDoTuChoi) || model.LyDoTuChoi.Trim().Length is < 5 or > 300)
            {
                TempData["Error"] = "Lý do từ chối cần từ 5 đến 300 ký tự.";
                return RedirectToAction(nameof(Pending));
            }
            recipe.TrangThai = "BiTuChoi";
            recipe.LyDoTuChoi = model.LyDoTuChoi.Trim();
        }
        else return BadRequest();

        await db.SaveChangesAsync();
        TempData["Success"] = decision == "approve" ? "Đã duyệt công thức." : "Đã từ chối công thức.";
        return RedirectToAction(nameof(Pending));
    }

    public async Task<IActionResult> Recipes(string? status)
    {
        var query = db.CongThuc.AsNoTracking().Include(x => x.DanhMuc).Include(x => x.NguoiDung).AsQueryable();
        if (status is "ChoDuyet" or "DaDuyet" or "BiTuChoi") query = query.Where(x => x.TrangThai == status);
        return View(new AdminRecipeListViewModel { Status = status, Recipes = await query.OrderByDescending(x => x.NgayTao).ToListAsync() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        var recipe = await db.CongThuc.SingleOrDefaultAsync(x => x.MaCongThuc == id);
        if (recipe is null) return NotFound();
        recipe.DaXoa = true;
        await db.SaveChangesAsync();
        TempData["Success"] = "Công thức đã được xóa.";
        return RedirectToAction(nameof(Recipes));
    }

    public async Task<IActionResult> Categories() => View(await db.DanhMuc.AsNoTracking().OrderBy(x => x.TenDanhMuc).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryInputViewModel model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Vui lòng nhập tên danh mục hợp lệ."; return RedirectToAction(nameof(Categories)); }
        var name = model.TenDanhMuc.Trim();
        if (await db.DanhMuc.IgnoreQueryFilters().AnyAsync(x => x.TenDanhMuc == name))
        {
            TempData["Error"] = "Tên danh mục đã tồn tại.";
            return RedirectToAction(nameof(Categories));
        }
        db.DanhMuc.Add(new DanhMuc { TenDanhMuc = name });
        await db.SaveChangesAsync();
        TempData["Success"] = "Danh mục đã được thêm.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryInputViewModel model)
    {
        if (!ModelState.IsValid || model.MaDanhMuc is null) { TempData["Error"] = "Tên danh mục chưa hợp lệ."; return RedirectToAction(nameof(Categories)); }
        var category = await db.DanhMuc.SingleOrDefaultAsync(x => x.MaDanhMuc == model.MaDanhMuc.Value);
        if (category is null) return NotFound();
        var name = model.TenDanhMuc.Trim();
        if (await db.DanhMuc.IgnoreQueryFilters().AnyAsync(x => x.MaDanhMuc != category.MaDanhMuc && x.TenDanhMuc == name))
        {
            TempData["Error"] = "Tên danh mục đã tồn tại.";
            return RedirectToAction(nameof(Categories));
        }
        category.TenDanhMuc = name;
        await db.SaveChangesAsync();
        TempData["Success"] = "Danh mục đã được cập nhật.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await db.DanhMuc.SingleOrDefaultAsync(x => x.MaDanhMuc == id);
        if (category is null) return NotFound();
        if (await db.CongThuc.IgnoreQueryFilters().AnyAsync(x => x.MaDanhMuc == id))
        {
            TempData["Error"] = "Không thể xóa danh mục đang có công thức.";
            return RedirectToAction(nameof(Categories));
        }
        category.DaXoa = true;
        await db.SaveChangesAsync();
        TempData["Success"] = "Danh mục đã được xóa.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Users() => View(await db.NguoiDung.AsNoTracking().OrderBy(x => x.VaiTro).ThenBy(x => x.HoTen).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == currentId)
        {
            TempData["Error"] = "Bạn không thể khóa chính tài khoản của mình.";
            return RedirectToAction(nameof(Users));
        }
        var user = await db.NguoiDung.SingleOrDefaultAsync(x => x.MaNguoiDung == id);
        if (user is null) return NotFound();
        user.HoatDong = !user.HoatDong;
        await db.SaveChangesAsync();
        TempData["Success"] = user.HoatDong ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        return RedirectToAction(nameof(Users));
    }
}
