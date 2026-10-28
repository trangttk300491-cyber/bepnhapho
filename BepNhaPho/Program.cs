using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BepNhaPho.Data;
using BepNhaPho.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    var messages = options.ModelBindingMessageProvider;
    messages.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập thông tin bắt buộc.");
    messages.SetAttemptedValueIsInvalidAccessor((_, field) => $"Giá trị của {field} chưa hợp lệ.");
    messages.SetUnknownValueIsInvalidAccessor(field => $"Giá trị của {field} chưa hợp lệ.");
    messages.SetValueIsInvalidAccessor(_ => "Giá trị nhập chưa hợp lệ.");
    messages.SetValueMustBeANumberAccessor(field => $"{field} phải là một số.");
    messages.SetMissingBindRequiredValueAccessor(field => $"Vui lòng nhập {field}.");
    messages.SetMissingKeyOrValueAccessor(() => "Vui lòng nhập đầy đủ thông tin.");
    messages.SetMissingRequestBodyRequiredValueAccessor(() => "Yêu cầu chưa có dữ liệu.");
    messages.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => "Giá trị nhập chưa hợp lệ.");
    messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Giá trị nhập chưa hợp lệ.");
    messages.SetNonPropertyValueMustBeANumberAccessor(() => "Vui lòng nhập một số hợp lệ.");
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<PasswordService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "BepNhaPho.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var active = int.TryParse(id, out var userId) &&
                await db.NguoiDung.AnyAsync(user => user.MaNguoiDung == userId && user.HoatDong);
            if (!active)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH(N'dbo.CongThuc', N'DaXoa') IS NULL
        BEGIN
            ALTER TABLE [dbo].[CongThuc]
            ADD [DaXoa] bit NOT NULL CONSTRAINT [DF_CongThuc_DaXoa] DEFAULT (0) WITH VALUES;
        END
        """);
    await db.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH(N'dbo.DanhMuc', N'DaXoa') IS NULL
        BEGIN
            ALTER TABLE [dbo].[DanhMuc]
            ADD [DaXoa] bit NOT NULL CONSTRAINT [DF_DanhMuc_DaXoa] DEFAULT (0) WITH VALUES;
        END
        """);
    await db.Database.ExecuteSqlRawAsync("""
        IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON c.user_type_id = t.user_type_id
                   WHERE c.object_id = OBJECT_ID(N'dbo.NguoiDung') AND c.name = N'HanMaDatLai' AND t.name = N'datetime2')
            ALTER TABLE [dbo].[NguoiDung] ALTER COLUMN [HanMaDatLai] datetime NULL;

        IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON c.user_type_id = t.user_type_id
                   WHERE c.object_id = OBJECT_ID(N'dbo.NguoiDung') AND c.name = N'NgayTao' AND t.name = N'datetime2')
            ALTER TABLE [dbo].[NguoiDung] ALTER COLUMN [NgayTao] datetime NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON c.user_type_id = t.user_type_id
                   WHERE c.object_id = OBJECT_ID(N'dbo.CongThuc') AND c.name = N'NgayTao' AND t.name = N'datetime2')
            ALTER TABLE [dbo].[CongThuc] ALTER COLUMN [NgayTao] datetime NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON c.user_type_id = t.user_type_id
                   WHERE c.object_id = OBJECT_ID(N'dbo.YeuThich') AND c.name = N'NgayTao' AND t.name = N'datetime2')
            ALTER TABLE [dbo].[YeuThich] ALTER COLUMN [NgayTao] datetime NOT NULL;
        """);
    await DemoData.SeedAsync(db, scope.ServiceProvider.GetRequiredService<PasswordService>());
}

app.Run();
