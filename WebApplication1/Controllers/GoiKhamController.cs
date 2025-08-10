using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using WebApplication1.Data;
using WebApplication1.ModelSTOs;

namespace WebApplication1.Controllers
{
    public class GoiKhamController : Controller
    {
        // gọi _dbService từ DI container
        private readonly AppDbContext _dbService;
        private readonly ILogger<GoiKhamController> _logger;

        public GoiKhamController(AppDbContext dbService, ILogger<GoiKhamController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        // gọi trang index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // lấy danh sách gói khám từ dbService
            var data = await _dbService.GoiKhamSTOs
                       .FromSqlRaw("EXEC sp_DanhSachGoiKhamBenhTheoNgay @IdChiNhanh",
                           new SqlParameter("@IdChiNhanh", 1))
                       .AsNoTracking()
                       .ToListAsync();
            return View(data);
        }

        // gọi các item đã lọc theo ngày
        [HttpPost]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay)
        {
            var data = await _dbService.GoiKhamSTOs
                .FromSqlRaw("EXEC sp_GoiKhamBenhLocTheoNgay @TuNgay, @DenNgay, @IdChiNhanh",
                    new SqlParameter("@TuNgay", tuNgay),
                    new SqlParameter("@DenNgay", denNgay),
                    new SqlParameter("@IdChiNhanh", 1))
                .AsNoTracking()
                .ToListAsync();

            string message = data.Any()
                ? $"Tìm thấy {data.Count} kết quả từ {tuNgay} đến {denNgay}."
                : $"Không tìm thấy kết quả nào từ {tuNgay} đến {denNgay}.";

            // Lưu cả dữ liệu VÀ ngày tháng vào Session
            var sessionData = new
            {
                Data = data,
                FromDate = tuNgay,
                ToDate = denNgay
            };

            HttpContext.Session.SetString("FilteredData", JsonConvert.SerializeObject(sessionData));
            return Json(new { success = true, message, data });
        }

        //[HttpPost("goikham/export/pdf")]
        //public IActionResult ExportToPDF([FromBody] ExportPdfRequest request)
        //{
        //    var document = new BaoCaoGoiKhamPDF(request.Data, request.FromDate, request.ToDate);
        //    var stream = new MemoryStream();
        //    document.GeneratePdf(stream);
        //    stream.Position = 0;
        //    return File(stream, "application/pdf", $"BaoCaoGoiKham_{request.FromDate}_den_{request.ToDate}.pdf");
        //}
        [HttpPost("goikham/export/pdf")]
        public async Task<IActionResult> ExportToPDF([FromBody] ExportPdfRequest request)
        {
            List<GoiKhamSTO> data;
            string fromDate;
            string toDate;

            // Nếu không có dữ liệu từ request (chưa lọc)
            if (request?.Data == null || request.Data.Count == 0)
            {
                // Lấy toàn bộ dữ liệu từ database
                data =  await _dbService.GoiKhamSTOs
                        .FromSqlRaw("EXEC sp_DanhSachGoiKhamBenhTheoNgay @IdChiNhanh",
                            new SqlParameter("@IdChiNhanh", 1))
                        .AsNoTracking()
                        .ToListAsync();

                // Tự động tính ngày bắt đầu (xa nhất) và ngày kết thúc (hiện tại)
                fromDate = data.Any()
                    ? data.Min(x => x.NgayDangKy).ToString("dd-MM-yyyy")
                    : DateTime.Now.ToString("dd-MM-yyyy");
                toDate = DateTime.Now.ToString("dd-MM-yyyy");
            }
            else
            {
                // Sử dụng dữ liệu đã lọc từ request
                data = request.Data;
                fromDate = request.FromDate;
                toDate = request.ToDate;
            }

            var document = new BaoCaoGoiKhamPDF(data, fromDate, toDate);
            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"BaoCaoGoiKham_{fromDate}_den_{toDate}.pdf");
        }
        public class ExportPdfRequest
        {
            public List<GoiKhamSTO> Data { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
        }
        [HttpPost("goikham/export/excel")]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportPdfRequest request)
        {
            List<GoiKhamSTO> data;
            string fromDate;
            string toDate;

            if (request?.Data == null || request.Data.Count == 0)
            {
                data = await _dbService.GoiKhamSTOs
                    .FromSqlRaw("EXEC sp_DanhSachGoiKhamBenhTheoNgay @IdChiNhanh",
                        new SqlParameter("@IdChiNhanh", 1))
                    .AsNoTracking()
                    .ToListAsync();

                fromDate = data.Any() ? data.Min(x => x.NgayDangKy).ToString("dd-MM-yyyy") : DateTime.Now.ToString("dd-MM-yyyy");
                toDate = DateTime.Now.ToString("dd-MM-yyyy");
            }
            else
            {
                data = request.Data;
                fromDate = request.FromDate;
                toDate = request.ToDate;
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo cáo Gói khám");

            // Thiết lập font chữ chung
            worksheet.Style.Font.FontName = "Times New Roman";
            worksheet.Style.Font.FontSize = 11;

            // --- Chèn logo ---
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dist", "img", "logo.png");
            if (System.IO.File.Exists(imagePath))
            {
                var image = worksheet.AddPicture(imagePath)
                                     .MoveTo(worksheet.Cell("A1"))
                                     .WithPlacement(XLPicturePlacement.FreeFloating);

                // Resize logo vừa phải (ví dụ 100x60 px)
                image.Width = 100;
                image.Height = 60;
            }

            // --- Bố trí thông tin bên cạnh logo ---
            worksheet.Cell("B1").Value = "BỆNH VIỆN A";
            worksheet.Cell("B1").Style.Font.Bold = true;
            worksheet.Cell("B1").Style.Font.FontSize = 12;

            worksheet.Cell("B2").Value = "Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM";
            worksheet.Cell("B2").Style.Font.FontSize = 10;

            worksheet.Cell("B3").Value = "Điện thoại: (028) 1234 5678";
            worksheet.Cell("B3").Style.Font.FontSize = 10;

            // --- Thông tin báo cáo, căn phải, ngang hàng với logo và thông tin bên trái ---
            worksheet.Range("E1:H1").Merge();
            worksheet.Cell("E1").Value = "BÁO CÁO THỰC HIỆN THEO DÕI GÓI KHÁM BỆNH";
            worksheet.Cell("E1").Style.Font.Bold = true;
            worksheet.Cell("E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell("E1").Style.Font.FontSize = 12;

            worksheet.Range("E2:H2").Merge();
            worksheet.Cell("E2").Value = "Đơn vị thống kê";
            worksheet.Cell("E2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell("E2").Style.Font.FontSize = 10;

            worksheet.Range("E3:H3").Merge();
            worksheet.Cell("E3").Value = $"Từ ngày: {fromDate} đến ngày: {toDate}";
            worksheet.Cell("E3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell("E3").Style.Font.FontSize = 10;

            // Định dạng cột rộng vừa phải cho phần header
            worksheet.Column("A").Width = 12;
            worksheet.Column("B").Width = 25;
            worksheet.Column("C").Width = 35;
            worksheet.Column("D").Width = 25;
            worksheet.Column("E").Width = 18;
            worksheet.Column("F").Width = 20;
            worksheet.Column("G").Width = 20;
            worksheet.Column("H").Width = 30;

            // Bắt đầu dữ liệu bảng từ dòng 6 để có khoảng cách với header
            int currentRow = 6;

            // Tiêu đề bảng
            worksheet.Cell(currentRow, 1).Value = "DANH SÁCH GÓI KHÁM BỆNH";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            worksheet.Range(currentRow, 1, currentRow, 8).Merge();
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow += 2;

            // Tiêu đề cột (tương tự code bạn đã có)
            worksheet.Cell(currentRow, 1).Value = "STT";
            worksheet.Cell(currentRow, 2).Value = "Mã y tế";
            worksheet.Cell(currentRow, 3).Value = "Họ tên";
            worksheet.Cell(currentRow, 4).Value = "Gói khám";
            worksheet.Cell(currentRow, 5).Value = "Ngày đăng ký";
            worksheet.Cell(currentRow, 6).Value = "Trạng thái";
            worksheet.Cell(currentRow, 7).Value = "Chỉ định còn lại";
            worksheet.Cell(currentRow, 8).Value = "Ghi chú";

            var headerRange = worksheet.Range(currentRow, 1, currentRow, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            currentRow++;

            // Ghi dữ liệu (giữ nguyên)
            int stt = 1;
            foreach (var item in data)
            {
                worksheet.Cell(currentRow, 1).Value = stt++;
                worksheet.Cell(currentRow, 2).Value = item.MaYTe;
                worksheet.Cell(currentRow, 3).Value = item.HoTen;
                worksheet.Cell(currentRow, 4).Value = item.GoiKham;
                worksheet.Cell(currentRow, 5).Value = item.NgayDangKy.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 6).Value = item.TrangThaiThucHien;
                worksheet.Cell(currentRow, 7).Value = item.ChiDinhConLai;
                worksheet.Cell(currentRow, 8).Value = item.GhiChu;

                var dataRange = worksheet.Range(currentRow, 1, currentRow, 8);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow++;
            }

            // Ngày tháng ở cuối
            worksheet.Cell(currentRow + 1, 8).Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
            worksheet.Cell(currentRow + 1, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(currentRow + 1, 8).Style.Font.FontSize = 10;

            // Bảng chữ ký (giữ nguyên)
            currentRow += 3;
            worksheet.Cell(currentRow, 2).Value = "THỦ TRƯỞNG ĐƠN VỊ";
            worksheet.Cell(currentRow, 4).Value = "THỦ QUỸ";
            worksheet.Cell(currentRow, 6).Value = "KẾ TOÁN";
            worksheet.Cell(currentRow, 8).Value = "NGƯỜI LẬP BẢNG";

            var signHeaderRange = worksheet.Range(currentRow, 2, currentRow, 8);
            signHeaderRange.Style.Font.Bold = true;
            signHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow++;

            worksheet.Cell(currentRow, 2).Value = "(Ký, họ tên, đóng dấu)";
            worksheet.Cell(currentRow, 4).Value = "(Ký, họ tên)";
            worksheet.Cell(currentRow, 6).Value = "(Ký, họ tên)";
            worksheet.Cell(currentRow, 8).Value = "(Ký, họ tên)";

            var signNoteRange = worksheet.Range(currentRow, 2, currentRow, 8);
            signNoteRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            signNoteRange.Style.Font.Italic = true;
            signNoteRange.Style.Font.FontSize = 9;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"BaoCaoGoiKham_{fromDate}_den_{toDate}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }



    }
}
