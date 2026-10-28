using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BepNhaPho.Data;
using BepNhaPho.ViewModels;

namespace BepNhaPho.Controllers;

public sealed class HomeController(AppDbContext db) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var recipes = await db.CongThuc.AsNoTracking()
            .Where(x => x.TrangThai == "DaDuyet")
            .Include(x => x.DanhMuc)
            .OrderByDescending(x => x.NgayTao)
            .Take(3)
            .ToListAsync();

        var favoriteIds = new List<int>();
        if (User.IsInRole("NguoiDung") && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            var recipeIds = recipes.Select(x => x.MaCongThuc).ToList();
            favoriteIds = await db.YeuThich.AsNoTracking()
                .Where(x => x.MaNguoiDung == userId && recipeIds.Contains(x.MaCongThuc))
                .Select(x => x.MaCongThuc)
                .ToListAsync();
        }

        return View(new RecipeListViewModel
        {
            Recipes = recipes.Select(recipe => new RecipeCardViewModel
            {
                Recipe = recipe,
                IsFavorite = favoriteIds.Contains(recipe.MaCongThuc)
            }).ToList(),
            Heading = "Món ngon mới được chia sẻ"
        });
    }

    [Route("Home/Error")]
    public IActionResult Error() => View();
}
