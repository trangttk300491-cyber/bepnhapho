using Microsoft.EntityFrameworkCore;
using BepNhaPho.Models;
using BepNhaPho.Services;

namespace BepNhaPho.Data;

public static class DemoData
{
    public static async Task SeedAsync(AppDbContext db, PasswordService passwords)
    {
        if (!db.DanhMuc.IgnoreQueryFilters().Any())
        {
            db.DanhMuc.AddRange(new[] { "Món Việt", "Món chay", "Món nước", "Món tráng miệng", "Món ăn sáng" }
                .Select(name => new DanhMuc { TenDanhMuc = name }));
            await db.SaveChangesAsync();
        }

        if (!db.NguoiDung.Any())
        {
            db.NguoiDung.AddRange(
                MakeUser("Quản trị Bếp Nhà Phố", "admin@bepnhapho.vn", "Admin1234", "QuanTri", passwords),
                MakeUser("An Nguyễn", "an.nguyen@example.com", "User1234", "NguoiDung", passwords),
                MakeUser("Minh Trần", "minh.tran@example.com", "User1234", "NguoiDung", passwords),
                MakeUser("Linh Phạm", "linh.pham@example.com", "User1234", "NguoiDung", passwords));
            await db.SaveChangesAsync();
        }

        if (!await db.NguoiDung.AnyAsync(x => x.Email == "trangttk300491@tvu-onschool.edu.vn"))
        {
            db.NguoiDung.Add(MakeUser("Trịnh Thị Kiều Trang", "trangttk300491@tvu-onschool.edu.vn",
                "123456", "NguoiDung", passwords));
            await db.SaveChangesAsync();
        }

        if (!db.CongThuc.IgnoreQueryFilters().Any())
        {
            var categories = db.DanhMuc.ToDictionary(x => x.TenDanhMuc, x => x.MaDanhMuc);
            var members = db.NguoiDung.Where(x => x.VaiTro == "NguoiDung").OrderBy(x => x.MaNguoiDung).Select(x => x.MaNguoiDung).ToArray();
            var data = new (string Title, string Category, int Minutes, int Servings, string Description, string[] Ingredients, string[] Steps)[]
            {
                ("Phở bò tái thơm ngọt", "Món nước", 90, 4, "Tô phở nóng với nước dùng trong, thơm mùi quế hồi và thịt bò mềm ngọt.", ["Bánh phở|400 g", "Thịt bò|300 g", "Xương bò|700 g", "Hành, gừng, quế hồi|vừa đủ"], ["Nướng thơm hành và gừng, hầm xương với quế hồi để lấy nước dùng.", "Trụng bánh phở, xếp thịt bò vào tô rồi chan nước dùng thật nóng."]),
                ("Bún chả Hà Nội", "Món Việt", 50, 4, "Thịt nướng xém cạnh ăn cùng bún tươi và bát nước chấm chua ngọt.", ["Thịt ba chỉ|500 g", "Thịt nạc vai|300 g", "Bún tươi|800 g", "Nước mắm, tỏi, chanh|vừa đủ"], ["Ướp thịt với nước mắm, đường và hành tỏi trong 30 phút.", "Nướng thịt vàng đều, pha nước chấm và dọn cùng bún, rau sống."]),
                ("Cá kho tộ nước màu", "Món Việt", 60, 3, "Cá kho đậm vị trong nồi đất, ăn rất hợp với cơm trắng nóng.", ["Cá basa|600 g", "Thịt ba chỉ|150 g", "Nước mắm|3 muỗng canh", "Tiêu, hành tím|vừa đủ"], ["Ướp cá với nước mắm, tiêu và hành tím khoảng 20 phút.", "Thắng nước màu, thêm thịt và cá, kho lửa nhỏ đến khi nước sệt."]),
                ("Gỏi cuốn tôm thịt", "Món Việt", 35, 4, "Cuốn tươi mát với tôm, thịt, rau thơm và nước chấm đậu phộng.", ["Tôm tươi|250 g", "Thịt ba chỉ|250 g", "Bánh tráng|12 cái", "Bún và rau sống|vừa đủ"], ["Luộc tôm và thịt, để nguội rồi thái thịt thành lát mỏng.", "Làm mềm bánh tráng, xếp bún rau tôm thịt và cuốn chặt tay."]),
                ("Canh chua cá lóc", "Món nước", 40, 4, "Vị chua thanh của me hòa cùng rau thơm, cá lóc mềm và nước canh nóng.", ["Cá lóc|500 g", "Cà chua|2 quả", "Thơm|1/4 quả", "Me, giá, rau om|vừa đủ"], ["Nấu me với nước, lọc lấy nước chua rồi đun sôi cùng cà chua và thơm.", "Cho cá vào nấu chín, thêm giá và rau om, nêm vừa ăn."]),
                ("Đậu hũ sốt cà chua", "Món chay", 25, 3, "Món chay giản dị với đậu hũ mềm và sốt cà chua chín mọng.", ["Đậu hũ trắng|3 miếng", "Cà chua|3 quả", "Hành lá|1 nhánh", "Dầu ăn, muối|vừa đủ"], ["Cắt đậu hũ thành miếng vuông và áp chảo vàng nhẹ.", "Nấu cà chua thành sốt, cho đậu vào rim vài phút rồi rắc hành lá."]),
                ("Chè khúc bạch hạnh nhân", "Món tráng miệng", 45, 4, "Chè mát lạnh với khúc bạch mềm mịn, vải ngọt và hạnh nhân rang.", ["Kem tươi|200 ml", "Sữa tươi|200 ml", "Vải|10 quả", "Hạnh nhân lát|30 g"], ["Ngâm gelatin, đun cùng sữa và kem trên lửa nhỏ rồi đổ khuôn làm lạnh.", "Cắt khúc bạch, thêm vải và nước đường, rắc hạnh nhân trước khi dùng."]),
                ("Bánh xèo miền Tây", "Món Việt", 45, 4, "Vỏ bánh vàng giòn ôm nhân tôm thịt, giá đỗ và rau sống tươi ngon.", ["Bột bánh xèo|300 g", "Tôm|200 g", "Thịt ba chỉ|200 g", "Giá đỗ|200 g"], ["Pha bột với nước cốt dừa và hành lá, chuẩn bị nhân tôm thịt.", "Tráng bột thật mỏng trên chảo nóng, thêm nhân và giá, gập đôi khi vỏ giòn."]),
                ("Bánh mì trứng ốp la", "Món ăn sáng", 15, 2, "Bữa sáng nhanh gọn với trứng lòng đào, bánh mì nóng và rau thơm.", ["Bánh mì|2 ổ", "Trứng gà|2 quả", "Dưa leo|1 quả", "Ngò và nước tương|vừa đủ"], ["Chiên trứng với chút dầu đến độ chín mong muốn.", "Kẹp trứng cùng dưa leo và rau thơm vào bánh mì, dùng khi còn nóng."]),
                ("Cháo gà nấm hương", "Món ăn sáng", 55, 4, "Cháo gà nóng mềm, thơm nấm hương, thích hợp cho bữa sáng gia đình.", ["Gạo tẻ|200 g", "Thịt gà|400 g", "Nấm hương|8 tai", "Hành lá, gừng|vừa đủ"], ["Luộc gà với gừng, vớt gà ra xé nhỏ và giữ lại phần nước luộc.", "Nấu gạo với nước luộc đến khi nở mềm, thêm nấm và thịt gà rồi nêm vừa ăn."])
            };

            var imagePaths = new Dictionary<string, string>
            {
                ["Phở bò tái thơm ngọt"] = "/img/recipes/pho-bo-tai.jpg",
                ["Bún chả Hà Nội"] = "/img/recipes/bun-cha-ha-noi.jpg",
                ["Cá kho tộ nước màu"] = "/img/recipes/ca-kho-to.jpg",
                ["Gỏi cuốn tôm thịt"] = "/img/recipes/goi-cuon-tom-thit.jpg",
                ["Canh chua cá lóc"] = "/img/recipes/canh-chua-ca-loc.jpg",
                ["Đậu hũ sốt cà chua"] = "/img/recipes/dau-hu-sot-ca-chua.jpg",
                ["Chè khúc bạch hạnh nhân"] = "/img/recipes/che-khuc-bach.jpg",
                ["Bánh xèo miền Tây"] = "/img/recipes/banh-xeo-mien-tay.jpg",
                ["Bánh mì trứng ốp la"] = "/img/recipes/banh-mi-trung-op-la.jpg",
                ["Cháo gà nấm hương"] = "/img/recipes/chao-ga-nam-huong.jpg"
            };

            for (var i = 0; i < data.Length; i++)
            {
                var item = data[i];
                db.CongThuc.Add(new CongThuc
                {
                    TieuDe = item.Title,
                    MoTa = item.Description,
                    AnhDaiDien = imagePaths[item.Title],
                    MaDanhMuc = categories[item.Category],
                    MaNguoiDung = members[i % members.Length],
                    ThoiGianNauPhut = item.Minutes,
                    KhauPhan = item.Servings,
                    TrangThai = "DaDuyet",
                    NgayTao = DateTime.UtcNow.AddDays(-i),
                    NguyenLieus = item.Ingredients.Select(pair => pair.Split('|')).Select(parts => new NguyenLieu { TenNguyenLieu = parts[0], DinhLuong = parts[1] }).ToList(),
                    BuocThucHiens = item.Steps.Select((step, index) => new BuocThucHien { SoThuTu = index + 1, NoiDung = step }).ToList()
                });
            }
            await db.SaveChangesAsync();
        }
    }

    private static NguoiDung MakeUser(string name, string email, string password, string role, PasswordService passwords) => new()
    {
        HoTen = name,
        Email = email,
        MatKhauMaHoa = passwords.Hash(password),
        VaiTro = role,
        HoatDong = true,
        NgayTao = DateTime.UtcNow
    };
}
