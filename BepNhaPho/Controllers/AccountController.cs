using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BepNhaPho.Data;
using BepNhaPho.Models;
using BepNhaPho.Services;
using BepNhaPho.ViewModels;

namespace BepNhaPho.Controllers;

public sealed class AccountController(AppDbContext db, PasswordService passwords) : Controller
{
    [AllowAnonymous]
    public IActionResult Register() => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!PasswordMeetsPolicy(model.MatKhau)) ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu cần ít nhất 8 ký tự, gồm cả chữ và số.");
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        if (await db.NguoiDung.AnyAsync(x => x.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
            return View(model);
        }
        db.NguoiDung.Add(new NguoiDung
        {
            HoTen = model.HoTen.Trim(),
            Email = email,
            MatKhauMaHoa = passwords.Hash(model.MatKhau),
            VaiTro = "NguoiDung",
            HoatDong = true,
            NgayTao = DateTime.UtcNow
        });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
            return View(model);
        }
        TempData["Success"] = "Đăng ký thành công. Mời bạn đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("QuanTri")) return RedirectToAction("Statistics", "Admin");
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl!);
            return RedirectToAction("Index", "Home");
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var user = await db.NguoiDung.SingleOrDefaultAsync(x => x.Email == email);
        if (user is null || !passwords.Verify(model.MatKhau, user.MatKhauMaHoa))
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }
        if (!user.HoatDong)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
            new(ClaimTypes.Name, user.HoTen),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.VaiTro)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        if (user.VaiTro == "QuanTri") return RedirectToAction("Statistics", "Admin");
        if (Url.IsLocalUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl!);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await db.NguoiDung.SingleOrDefaultAsync(x => x.Email == model.Email.Trim().ToLowerInvariant());
        if (user is null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email không tồn tại trong hệ thống.");
            return View(model);
        }
        var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        user.MaDatLaiMatKhau = code;
        user.HanMaDatLai = DateTime.UtcNow.AddMinutes(30);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(ResetPassword), new { code });
    }

    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string? code)
    {
        var user = string.IsNullOrWhiteSpace(code) ? null : await db.NguoiDung.SingleOrDefaultAsync(x => x.MaDatLaiMatKhau == code && x.HanMaDatLai > DateTime.UtcNow);
        if (user is null)
        {
            TempData["Error"] = "Mã đặt lại không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.";
            return RedirectToAction(nameof(ForgotPassword));
        }
        return View(new ResetPasswordViewModel { MaDatLaiMatKhau = code! });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!PasswordMeetsPolicy(model.MatKhauMoi)) ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu cần ít nhất 8 ký tự, gồm cả chữ và số.");
        var user = await db.NguoiDung.SingleOrDefaultAsync(x => x.MaDatLaiMatKhau == model.MaDatLaiMatKhau && x.HanMaDatLai > DateTime.UtcNow);
        if (user is null)
        {
            TempData["Error"] = "Mã đặt lại không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.";
            return RedirectToAction(nameof(ForgotPassword));
        }
        if (!ModelState.IsValid) return View(model);
        user.MatKhauMaHoa = passwords.Hash(model.MatKhauMoi);
        user.MaDatLaiMatKhau = null;
        user.HanMaDatLai = null;
        await db.SaveChangesAsync();
        TempData["Success"] = "Mật khẩu đã được đặt lại. Mời bạn đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = "NguoiDung")]
    public async Task<IActionResult> Profile()
    {
        var user = await CurrentUserAsync();
        return View(new ProfileViewModel { HoTen = user.HoTen, Email = user.Email });
    }

    [HttpPost, Authorize(Roles = "NguoiDung"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) { model.Email = User.FindFirstValue(ClaimTypes.Email) ?? ""; return View(model); }
        var user = await CurrentUserAsync();
        user.HoTen = model.HoTen.Trim();
        await db.SaveChangesAsync();
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, BuildPrincipal(user));
        TempData["Success"] = "Hồ sơ đã được cập nhật.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost, Authorize(Roles = "NguoiDung"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!PasswordMeetsPolicy(model.MatKhauMoi)) ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu cần ít nhất 8 ký tự, gồm cả chữ và số.");
        if (!ModelState.IsValid)
        {
            var user = await CurrentUserAsync();
            return View("Profile", new ProfileViewModel { HoTen = user.HoTen, Email = user.Email });
        }
        var current = await CurrentUserAsync();
        if (!passwords.Verify(model.MatKhauCu, current.MatKhauMaHoa))
        {
            ModelState.AddModelError(nameof(model.MatKhauCu), "Mật khẩu hiện tại không đúng.");
            return View("Profile", new ProfileViewModel { HoTen = current.HoTen, Email = current.Email });
        }
        current.MatKhauMaHoa = passwords.Hash(model.MatKhauMoi);
        await db.SaveChangesAsync();
        TempData["Success"] = "Mật khẩu đã được thay đổi.";
        return RedirectToAction(nameof(Profile));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private Task<NguoiDung> CurrentUserAsync() => db.NguoiDung.SingleAsync(x => x.MaNguoiDung == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

    private static bool PasswordMeetsPolicy(string? password) => !string.IsNullOrEmpty(password) && password.Length >= 8 && password.Any(char.IsLetter) && password.Any(char.IsDigit);

    private static ClaimsPrincipal BuildPrincipal(NguoiDung user) => new(new ClaimsIdentity(
    [new(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()), new(ClaimTypes.Name, user.HoTen), new(ClaimTypes.Email, user.Email), new(ClaimTypes.Role, user.VaiTro)],
    CookieAuthenticationDefaults.AuthenticationScheme));
}
