# Bếp Nhà Phố — Website chia sẻ công thức nấu ăn

## 1. Tổng quan dự án

Bếp Nhà Phố giúp người dùng tìm cách chế biến món ăn, chia sẻ công thức và lưu các món yêu thích. Công thức gồm tiêu đề, mô tả, ảnh đại diện, danh mục, thời gian nấu, khẩu phần, nguyên liệu và các bước thực hiện.

Công thức thành viên gửi phải được quản trị viên duyệt trước khi công khai. Bài được chỉnh sửa sẽ trở về trạng thái chờ duyệt.

| Nhóm người sử dụng | Chức năng |
|---|---|
| Khách chưa đăng nhập | Xem công thức đã duyệt, tìm theo tên món/nguyên liệu, lọc danh mục; đăng ký, đăng nhập và quên mật khẩu |
| Thành viên (`NguoiDung`) | Đăng, sửa và xóa mềm bài của mình; xem trạng thái/lý do từ chối; quản lý yêu thích; cập nhật họ tên và đổi mật khẩu |
| Quản trị viên (`QuanTri`) | Giao diện quản trị riêng; duyệt/từ chối bài, quản lý công thức và danh mục; khóa/mở khóa tài khoản; xem thống kê công thức, danh mục và tài khoản |

Trang chủ giới thiệu website và hiển thị 3 công thức mới nhất. Tab Công thức có danh sách đầy đủ, tìm kiếm, bộ lọc và phân trang tối đa 12 mục mỗi trang.

Trang Thống kê trong khu vực quản trị hiển thị số công thức theo trạng thái duyệt, số tài khoản đang hoạt động/đã khóa và số công thức theo danh mục. Số liệu được tổng hợp khi mở hoặc tải lại trang; công thức và danh mục đã xóa mềm không được tính.

### Công nghệ và dữ liệu

- **Backend:** C#, ASP.NET Core MVC trên .NET 8.
- **Giao diện:** Razor View (`.cshtml`), HTML, CSS, Bootstrap 5, JavaScript; jQuery Validation trên một số biểu mẫu.
- **Dữ liệu:** Entity Framework Core 8, SQL Server, phương pháp Code First.
- **Xác thực:** Cookie Authentication; mật khẩu băm PBKDF2-SHA256.
- **Ảnh:** JPG/JPEG hoặc PNG, tối đa 2 MB; không nhận WEBP. Kiểm tra chữ ký định dạng, đổi tên GUID, lưu tại `wwwroot/uploads/recipes`.
- **Xóa mềm:** công thức và danh mục được đánh dấu bằng cột `DaXoa`.

Cơ sở dữ liệu có 6 bảng: `NguoiDung`, `DanhMuc`, `CongThuc`, `NguyenLieu`, `BuocThucHien`, `YeuThich`. Các cột thời gian dùng kiểu `datetime`.

Luồng quên mật khẩu hiện phục vụ đồ án: email tồn tại thì chuyển trực tiếp đến trang đặt mật khẩu mới. Mã có hiệu lực 30 phút và dùng một lần. Ứng dụng chưa gửi email xác minh quyền sở hữu tài khoản.

## 2. Yêu cầu cài đặt

1. **.NET 8 SDK** để restore, build và chạy mã nguồn. Chỉ cài Runtime là chưa đủ.
2. **SQL Server Database Engine** đang chạy, hoặc SQL Server khác mà máy có thể kết nối. Có thể dùng bản Express hoặc Developer cho môi trường học tập.
3. **Visual Studio Code** và extension **C# Dev Kit** nếu muốn chạy/debug trong VS Code.
4. **Trình duyệt** để truy cập website.
5. **Internet** cho lần tải gói NuGet và tài nguyên Bootstrap/font/jQuery từ CDN.

SQL Server Management Studio (SSMS) là công cụ quản lý tùy chọn; cài SSMS không thay thế việc cài SQL Server Database Engine.

Kiểm tra SDK trong terminal:

```powershell
dotnet --list-sdks
```

Cần có SDK .NET 8. Mở lại terminal/VS Code sau khi cài nếu chưa nhận lệnh `dotnet`.

## 3. Cấu hình kết nối SQL Server

Mở `appsettings.json`, sửa `ConnectionStrings:DefaultConnection` theo SQL Server thực tế. Tên máy/instance trong mã nguồn hiện tại thuộc máy phát triển nên cần đổi khi chuyển sang máy khách.

### Dùng tài khoản SQL Server

Thay các giá trị mẫu bằng thông tin của máy cần chạy:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TEN_MAY\\TEN_INSTANCE;Database=CongThucNauAn;User Id=TAI_KHOAN_SQL;Password=MAT_KHAU_SQL;TrustServerCertificate=True;Encrypt=False;"
}
```

Không bắt buộc dùng `sa`. Tài khoản SQL phải có quyền phù hợp. SQL Server cần cho phép SQL Server Authentication nếu dùng cách này.

### Dùng Windows Authentication

Ví dụ SQL Server Express trên cùng máy:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CongThucNauAn;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
}
```

Ứng dụng kết nối bằng tài khoản Windows đang chạy tiến trình. Tài khoản đó cần được cấp quyền trong SQL Server. Với instance mặc định trên cùng máy, có thể dùng `Server=localhost`.

Trong JSON, dấu `\` phải viết thành `\\`. Chỉ thay khối `ConnectionStrings`, giữ nguyên các phần cấu hình khác. `TrustServerCertificate=True;Encrypt=False` trong ví dụ phục vụ môi trường chạy local của dự án.

### Ghi đè bằng biến môi trường

Có thể cấu hình trong terminal PowerShell trước khi chạy mà không sửa file:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=CongThucNauAn;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;'
```

Chuỗi PowerShell dùng một dấu `\`. Biến môi trường ghi đè `appsettings.json`, áp dụng cho terminal hiện tại và các tiến trình khởi chạy từ terminal đó.

### Khởi tạo database

Khi khởi động, ứng dụng dùng `EnsureCreated` để tạo database/bảng khi cần, chạy các kiểm tra cập nhật cấu trúc đã khai báo trong `Program.cs`, rồi thêm dữ liệu demo nếu các bảng tương ứng đang trống.

- Nên dùng database mới dành riêng cho dự án, ví dụ `CongThucNauAn`.
- Không cần chạy EF migrations; dự án hiện chưa sử dụng migrations.
- Tài khoản kết nối cần quyền tạo database nếu chưa tồn tại, tạo/cập nhật cấu trúc bảng và đọc/ghi dữ liệu.
- Nếu không có quyền tạo database, người quản lý SQL Server cần chuẩn bị database và cấp quyền phù hợp trước khi chạy.

## 4. Cài đặt và chạy bằng terminal

### Bước 1 — Mở thư mục dự án

Giải nén hoặc sao chép thư mục `BepNhaPho`. Mở terminal tại nơi chứa `BepNhaPho.csproj`.


Trên máy khách, thay bằng đường dẫn thực tế. Hoàn thành cấu hình database ở mục 3 và bảo đảm dịch vụ SQL Server đang chạy. Có thể thử kết nối bằng SSMS với cùng thông tin.

### Bước 2 — Chuẩn bị HTTPS local

```powershell
dotnet dev-certs https --trust
```

Lệnh này tạo/tin cậy chứng chỉ phát triển trên máy chạy ứng dụng. Chấp nhận hộp thoại tin cậy chứng chỉ nếu hệ điều hành hiển thị.

### Bước 3 — Tải thư viện và build

```powershell
dotnet restore
dotnet build
```

### Bước 4 — Chạy website

```powershell
dotnet run --launch-profile BepNhaPho
```

Khi terminal hiển thị `Application started` và `Now listening on`, mở:

| Trang | Địa chỉ |
|---|---|
| Trang chủ | https://localhost:7018/ |
| HTTP | http://localhost:5018/ |
| Đăng nhập | https://localhost:7018/Account/Login |
| Quản trị (trang đích sau khi admin đăng nhập) | https://localhost:7018/Admin/Statistics |
| Công thức chờ duyệt | https://localhost:7018/Admin/Pending |
| Thống kê quản trị | https://localhost:7018/Admin/Statistics |

Ứng dụng có chuyển hướng HTTP sang HTTPS nên ưu tiên địa chỉ HTTPS. Trang quản trị yêu cầu tài khoản có vai trò `QuanTri`.

Giữ terminal đang chạy để website hoạt động. Nhấn **Ctrl+C** để dừng trước khi build hoặc chạy lại.

## 5. Chạy và debug bằng Visual Studio Code

1. Chọn **File → Open Folder**, mở đúng thư mục **BepNhaPho**, nơi có `BepNhaPho.csproj` và `.vscode`.
2. Cài extension **C# Dev Kit**.
3. Cấu hình kết nối SQL Server và chứng chỉ HTTPS theo các mục trên.
4. Vào **Run and Debug**, chọn cấu hình **BepNhaPho**, nhấn **F5**.
5. VS Code thực hiện tác vụ build rồi chạy ứng dụng trên cổng 7018/5018.

Dự án có sẵn `.vscode/launch.json` và `.vscode/tasks.json`. Giữ thư mục `.vscode` khi chuyển mã nguồn để dùng cấu hình này. Có thể chạy bằng terminal theo mục 4 nếu không dùng debugger.

Nếu dùng Visual Studio, mở `BepNhaPho.sln`, chọn dự án `BepNhaPho` và chạy F5. Cần Visual Studio hỗ trợ .NET 8 và workload **ASP.NET and web development**.

## 6. Tài khoản và dữ liệu demo

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Quản trị viên | `admin@bepnhapho.vn` | `Admin1234` |
| Thành viên | `an.nguyen@example.com` | `User1234` |
| Thành viên | `minh.tran@example.com` | `User1234` |
| Thành viên | `linh.pham@example.com` | `User1234` |
| Thành viên | `trangttk300491@tvu-onschool.edu.vn` | `trang123@` |

## 7. Cấu trúc và chuyển dự án sang máy khác

| Thư mục/tệp | Nội dung |
|---|---|
| `Controllers` | Tiếp nhận yêu cầu và xử lý nghiệp vụ |
| `Models` | Các thực thể dữ liệu |
| `ViewModels` | Dữ liệu biểu mẫu và màn hình |
| `Views` | Giao diện Razor `.cshtml` |
| `Views/Shared` | Layout và thành phần dùng chung |
| `Data` | AppDbContext và dữ liệu demo |
| `Services` | Dịch vụ mật khẩu |
| `wwwroot` | CSS, JavaScript và ảnh |
| `wwwroot/uploads/recipes` | Ảnh công thức tải lên |
| `Properties` | Cấu hình khởi chạy |
| `.vscode` | Cấu hình chạy/debug VS Code |
| `Program.cs` | Cấu hình ứng dụng và khởi tạo dữ liệu |
| `appsettings.json` | Chuỗi kết nối và cấu hình ứng dụng |
| `BepNhaPho.csproj` | Tệp dự án .NET |
| `BepNhaPho.sln` | Tệp solution |

Khi chuyển máy, sao chép đầy đủ mã nguồn, `Views`, `wwwroot`, cấu hình và `.vscode`; sửa chuỗi kết nối theo SQL Server máy khách. Có thể bỏ `bin`, `obj`, `.vs`; máy khách sẽ tạo lại khi build.

Sao chép mã nguồn không mang theo dữ liệu SQL Server. Muốn giữ dữ liệu hiện có thì cần backup/restore database riêng và giữ thư mục `wwwroot/uploads/recipes`. Nếu dùng database mới, ứng dụng tạo dữ liệu demo theo các điều kiện đã nêu.
