using System.ComponentModel.DataAnnotations;

namespace BepNhaPho.ViewModels;

public sealed class RegisterViewModel
{
    [Display(Name = "Họ và tên"), Required(ErrorMessage = "Vui lòng nhập họ tên."), StringLength(100, ErrorMessage = "Họ và tên không được vượt quá {1} ký tự.")] public string HoTen { get; set; } = "";
    [Display(Name = "Email"), Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), StringLength(256, ErrorMessage = "Email không được vượt quá {1} ký tự.")] public string Email { get; set; } = "";
    [Display(Name = "Mật khẩu"), Required(ErrorMessage = "Vui lòng nhập mật khẩu."), MinLength(8, ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự."), StringLength(100, ErrorMessage = "Mật khẩu không được vượt quá {1} ký tự.")] public string MatKhau { get; set; } = "";
    [Display(Name = "Mật khẩu nhập lại"), Required(ErrorMessage = "Vui lòng nhập mật khẩu nhập lại."), Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu nhập lại chưa khớp.")] public string NhapLaiMatKhau { get; set; } = "";
}

public sealed class LoginViewModel
{
    [Display(Name = "Email"), Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")] public string Email { get; set; } = "";
    [Display(Name = "Mật khẩu"), Required(ErrorMessage = "Vui lòng nhập mật khẩu.")] public string MatKhau { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public sealed class ForgotPasswordViewModel
{
    [Display(Name = "Email"), Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")] public string Email { get; set; } = "";
}

public sealed class ResetPasswordViewModel
{
    [Display(Name = "Mã đặt lại mật khẩu"), Required(ErrorMessage = "Vui lòng nhập mã đặt lại mật khẩu.")] public string MaDatLaiMatKhau { get; set; } = "";
    [Display(Name = "Mật khẩu mới"), Required(ErrorMessage = "Vui lòng nhập mật khẩu mới."), MinLength(8, ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự."), StringLength(100, ErrorMessage = "Mật khẩu mới không được vượt quá {1} ký tự.")] public string MatKhauMoi { get; set; } = "";
    [Display(Name = "Mật khẩu nhập lại"), Required(ErrorMessage = "Vui lòng nhập mật khẩu nhập lại."), Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại chưa khớp.")] public string NhapLaiMatKhau { get; set; } = "";
}

public sealed class ProfileViewModel
{
    [Display(Name = "Họ và tên"), Required(ErrorMessage = "Vui lòng nhập họ tên."), StringLength(100, ErrorMessage = "Họ và tên không được vượt quá {1} ký tự.")] public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class ChangePasswordViewModel
{
    [Display(Name = "Mật khẩu hiện tại"), Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")] public string MatKhauCu { get; set; } = "";
    [Display(Name = "Mật khẩu mới"), Required(ErrorMessage = "Vui lòng nhập mật khẩu mới."), MinLength(8, ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự."), StringLength(100, ErrorMessage = "Mật khẩu mới không được vượt quá {1} ký tự.")] public string MatKhauMoi { get; set; } = "";
    [Display(Name = "Mật khẩu nhập lại"), Required(ErrorMessage = "Vui lòng nhập mật khẩu nhập lại."), Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại chưa khớp.")] public string NhapLaiMatKhau { get; set; } = "";
}
