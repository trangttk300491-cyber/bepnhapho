using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BepNhaPho.Data;
using BepNhaPho.Models;

namespace BepNhaPho.Controllers;

[Authorize(Roles = "NguoiDung")]
public sealed class FavoriteController(AppDbContext db) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int recipeId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recipe = await db.CongThuc.SingleOrDefaultAsync(x => x.MaCongThuc == recipeId && x.TrangThai == "DaDuyet");
        if (recipe is null) return NotFound();
        var favorite = await db.YeuThich.SingleOrDefaultAsync(x => x.MaCongThuc == recipeId && x.MaNguoiDung == userId);
        if (favorite is null)
        {
            db.YeuThich.Add(new YeuThich { MaCongThuc = recipeId, MaNguoiDung = userId, NgayTao = DateTime.UtcNow });
            await db.SaveChangesAsync();
            return Json(new { isFavorite = true });
        }
        db.YeuThich.Remove(favorite);
        await db.SaveChangesAsync();
        return Json(new { isFavorite = false });
    }
}
