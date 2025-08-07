using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using System.Globalization;
using System.Globalization;
using System.Reflection.Metadata;
using WebApplication1.Data;
using WebApplication1.Models;
namespace WebApplication1.Controllers
{
    public class BenhNhanController : Controller
    {
        private readonly AppDbContext _dbService;
        private readonly ILogger<BenhNhanController> _logger;

        public BenhNhanController(AppDbContext dbService, ILogger<BenhNhanController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var danhSach = _dbService.BenhNhans
                .Include(bn => bn.Nguoi) // Liên kết với bảng Nguoi nếu có
                .ToList();

            return View(danhSach);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new BenhNhan
            {
                Nguoi = new Nguoi(),
                NgayNhapVien = DateTime.Now
            };

            // Lấy danh sách dân tộc từ DB
            var danTocList = await _dbService.DanTocs.ToListAsync();
            ViewBag.DanTocList = danTocList; // KHÔNG dùng SelectList

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Add(BenhNhan model, int currentPage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý Tỉnh/Thành
                    if (!string.IsNullOrEmpty(model.Nguoi.TinhThanh?.MaTinh))
                    {
                        // Kiểm tra tỉnh đã tồn tại chưa
                        var existingTinhThanh = await _dbService.TinhThanhs
                            .FirstOrDefaultAsync(tt => tt.MaTinh == model.Nguoi.TinhThanh.MaTinh);

                        if (existingTinhThanh == null)
                        {
                            // Tạo mới TinhThanh từ dữ liệu form
                            var newTinhThanh = new TinhThanh
                            {
                                MaTinh = model.Nguoi.TinhThanh.MaTinh,
                                TenTinh = model.Nguoi.TinhThanh.TenTinh,
                                VietTat = model.Nguoi.TinhThanh.VietTat
                            };

                            await _dbService.TinhThanhs.AddAsync(newTinhThanh);
                            await _dbService.SaveChangesAsync();

                            model.Nguoi.TinhThanhId = newTinhThanh.Id;
                        }
                        else
                        {
                            model.Nguoi.TinhThanhId = existingTinhThanh.Id;
                        }

                        // Đảm bảo không lưu đối tượng TinhThanh mới qua navigation property
                        model.Nguoi.TinhThanh = null;
                    }

                    // Tiếp tục xử lý lưu bệnh nhân...
                    await _dbService.Nguois.AddAsync(model.Nguoi);
                    await _dbService.SaveChangesAsync();

                    model.MaNguoi = model.Nguoi.MaNguoi;
                    model.DonGia = 0;
                    await _dbService.BenhNhans.AddAsync(model);
                    await _dbService.SaveChangesAsync();

                    return RedirectToAction("DanhSach", new { page = currentPage, pageSize = ViewBag.PageSize });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thêm bệnh nhân.");
                    return Json(new { success = false, errors = new { Global = "Lỗi khi thêm bệnh nhân: " + ex.Message } });
                }
            }

            var errorList = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.First().ErrorMessage
            );
            return Json(new { success = false, errors = errorList });
        }

        [HttpGet("BenhNhan/Edit/{MaBenhNhan}")]
        public async Task<IActionResult> Edit(string MaBenhNhan)
        {
            try
            {
                var benhNhan = await _dbService.BenhNhans
                    .Include(b => b.Nguoi)
                    .ThenInclude(n => n.DanToc)
                    .Include(b => b.Nguoi)
                    .ThenInclude(n => n.TinhThanh)
                    .FirstOrDefaultAsync(b => b.MaBenhNhan == MaBenhNhan);

                if (benhNhan == null)
                {
                    return NotFound();
                }

                // Kiểm tra null và khởi tạo danh sách rỗng nếu cần
                var danTocList = await _dbService?.DanTocs?.ToListAsync() ?? new List<DanToc>();
                ViewBag.DanTocList = danTocList;

                return View(benhNhan);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(ex, "Lỗi khi tải trang chỉnh sửa bệnh nhân");

                // Trả về trang lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi tải dữ liệu");
            }
        }

        [HttpPost("BenhNhan/Edit/{MaBenhNhan}")]
        public async Task<IActionResult> Edit(string MaBenhNhan, BenhNhan model, int currentPage = 1, int pageSize = 5)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DanTocList = await _dbService.DanTocs.ToListAsync();
                return View(model);
            }

            var existingBenhNhan = await _dbService.BenhNhans
                .Include(bn => bn.Nguoi)
                .FirstOrDefaultAsync(bn => bn.MaBenhNhan == MaBenhNhan);

            if (existingBenhNhan == null)
            {
                return NotFound();
            }

            // Xử lý Tỉnh/Thành - Tạo mới nếu không tồn tại
            if (!string.IsNullOrEmpty(model.Nguoi.TinhThanh?.MaTinh))
            {
                // Kiểm tra theo MaTinh (khóa duy nhất)
                var existingTinhThanh = await _dbService.TinhThanhs
                    .FirstOrDefaultAsync(tt => tt.MaTinh == model.Nguoi.TinhThanh.MaTinh);

                if (existingTinhThanh == null)
                {
                    // Tạo mới Tỉnh/Thành
                    var newTinhThanh = new TinhThanh
                    {
                        MaTinh = model.Nguoi.TinhThanh.MaTinh,
                        TenTinh = model.Nguoi.TinhThanh.TenTinh,
                        VietTat = model.Nguoi.TinhThanh.VietTat
                    };

                    // Bắt đầu transaction để đảm bảo toàn vẹn dữ liệu
                    using var transaction = await _dbService.Database.BeginTransactionAsync();

                    try
                    {
                        await _dbService.TinhThanhs.AddAsync(newTinhThanh);
                        await _dbService.SaveChangesAsync(); // Lưu để có ID

                        existingBenhNhan.Nguoi.TinhThanhId = newTinhThanh.Id;

                        // Cập nhật thông tin khác
                        UpdateBenhNhanInfo(existingBenhNhan, model);

                        await _dbService.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Lỗi khi tạo mới Tỉnh/Thành");
                        ModelState.AddModelError("", "Lỗi khi lưu Tỉnh/Thành mới");
                        ViewBag.DanTocList = await _dbService.DanTocs.ToListAsync();
                        return View(model);
                    }
                }
                else
                {
                    // Nếu đã tồn tại, gán ID
                    existingBenhNhan.Nguoi.TinhThanhId = existingTinhThanh.Id;
                    UpdateBenhNhanInfo(existingBenhNhan, model);
                    await _dbService.SaveChangesAsync();
                }
            }
            else
            {
                // Nếu không có Tỉnh/Thành được chọn
                existingBenhNhan.Nguoi.TinhThanhId = null;
                UpdateBenhNhanInfo(existingBenhNhan, model);
                await _dbService.SaveChangesAsync();
            }

            return RedirectToAction("DanhSach", new
            {
                page = currentPage > 0 ? currentPage : 1,
                pageSize = pageSize > 0 ? pageSize : 5
            });
        }
        // Hàm riêng để cập nhật thông tin
        private void UpdateBenhNhanInfo(BenhNhan existing, BenhNhan model)
        {
            existing.Nguoi.HoTen = model.Nguoi.HoTen;
            existing.Nguoi.GioiTinh = model.Nguoi.GioiTinh;
            existing.Nguoi.NgaySinh = model.Nguoi.NgaySinh;
            existing.Nguoi.DanTocId = model.Nguoi.DanTocId;

            existing.NgayNhapVien = model.NgayNhapVien;
            existing.NgayXuatVien = model.NgayXuatVien;
            existing.DonGia = model.DonGia;

            if (model.NgayXuatVien.HasValue)
            {
                var ngayNhap = model.NgayNhapVien.Date;
                var ngayXuat = model.NgayXuatVien.Value.Date;
                existing.SoNgayNhapVien = (ngayXuat - ngayNhap).Days + 1;
                existing.TongTien = existing.SoNgayNhapVien * (model.DonGia ?? 0);
            }
        }


        [HttpPost]
        public IActionResult ThongTin(BenhNhan model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý thêm mới
                    _dbService.Nguois.Add(model.Nguoi);
                    _dbService.SaveChanges();

                    _dbService.BenhNhans.Add(model);
                    _dbService.SaveChanges();



                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi thêm bệnh nhân: " + ex.Message);
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id, int currentPage)
        {
            try
            {
                var benhNhan = await _dbService.BenhNhans
                    .Include(bn => bn.Nguoi)
                    .FirstOrDefaultAsync(bn => bn.MaBenhNhan == id);

                if (benhNhan == null)
                {
                    return NotFound();
                }

                if (benhNhan.Nguoi != null)
                {
                    _dbService.Nguois.Remove(benhNhan.Nguoi);
                }

                _dbService.BenhNhans.Remove(benhNhan);
                await _dbService.SaveChangesAsync();

                // Giữ nguyên trang hiện tại khi redirect
                return RedirectToAction("DanhSach", new { page = currentPage, pageSize = ViewBag.PageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bệnh nhân");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bệnh nhân" });
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> DanhSach(int page = 1, int pageSize = 5)
        //{
        //    var totalItems = await _dbService.BenhNhans.CountAsync();
        //    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        //    // Đảm bảo page không vượt quá totalPages
        //    page = Math.Max(1, Math.Min(page, totalPages));

        //    var danhSach = await _dbService.BenhNhans
        //    .Include(bn => bn.Nguoi)
        //        .ThenInclude(n => n.DanToc) // Load DanToc
        //    .Include(bn => bn.Nguoi)        // Bắt đầu lại từ Nguoi
        //        .ThenInclude(n => n.TinhThanh) // Load TinhThanh
        //    .Skip((page - 1) * pageSize)
        //    .Take(pageSize)
        //    .ToListAsync();

        //    ViewBag.CurrentPage = page;
        //    ViewBag.TotalPages = totalPages;
        //    ViewBag.PageSize = pageSize;

        //    return View(danhSach);
        //}
        [HttpGet]
        public async Task<IActionResult> DanhSach(int page = 1, int pageSize = 5, bool decimalFormat = false)
        {
            var totalItems = await _dbService.BenhNhans.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var danhSach = await _dbService.BenhNhans
                .Include(bn => bn.Nguoi)
                .ThenInclude(n => n.DanToc)
                .Include(bn => bn.Nguoi)
                .ThenInclude(n => n.TinhThanh)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.DecimalFormat = decimalFormat; // Truyền giá trị sang view
            //sd 
            // sdg

            return View(danhSach);
        }

        [HttpGet]
        public async Task<IActionResult> SearchBenhNhan(string tenBenhNhan, int page = 1, int pageSize = 5)
        {
            var query = _dbService.BenhNhans
                .Include(bn => bn.Nguoi)
                .ThenInclude(n => n.DanToc)
                .Include(bn => bn.Nguoi)
                .ThenInclude(n => n.TinhThanh)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tenBenhNhan))
            {
                tenBenhNhan = tenBenhNhan.ToLower();
                query = query.Where(bn => bn.Nguoi.HoTen.ToLower().Contains(tenBenhNhan));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var danhSach = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bn => new
                {
                    MaBenhNhan = bn.MaBenhNhan,
                    HoTen = bn.Nguoi.HoTen,
                    NgaySinh = bn.Nguoi.NgaySinh.HasValue ? bn.Nguoi.NgaySinh.Value.ToString("dd-MM-yyyy") : "",
                    GioiTinh = bn.Nguoi.GioiTinh,
                    DanToc = bn.Nguoi.DanToc != null ? bn.Nguoi.DanToc.TenDanToc : "Không rõ",
                    TinhThanh = bn.Nguoi.TinhThanh != null ? bn.Nguoi.TinhThanh.TenTinh : "Không rõ",
                    NgayNhapVien = bn.NgayNhapVien.ToString("dd-MM-yyyy HH:mm"),
                    NgayXuatVien = bn.NgayXuatVien.HasValue ? bn.NgayXuatVien.Value.ToString("dd-MM-yyyy") : "",
                    SoNgay = bn.SoNgayNhapVien,
                    DonGia = bn.DonGia.HasValue ? bn.DonGia.Value.ToString("N0") : "",
                    TongTien = bn.TongTien.HasValue ? bn.TongTien.Value.ToString("N0") : ""
                })
                .ToListAsync();

            return Json(new
            {
                Data = danhSach,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPDF()
        {
            try
            {
                var data = await _dbService.BenhNhans
                    .Include(b => b.Nguoi)
                        .ThenInclude(n => n.DanToc)
                    .Include(b => b.Nguoi)
                        .ThenInclude(n => n.TinhThanh)
                    .AsNoTracking()
                    .ToListAsync();

                if (data == null || !data.Any())
                {
                    return BadRequest("Không có dữ liệu bệnh nhân để xuất PDF");
                }

                var document = new BenhNhanListPDF(data);
                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", "DanhSachBenhNhan.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất PDF");
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var data = _dbService.BenhNhans
                .Include(b => b.Nguoi)
                .Include(b => b.Nguoi.DanToc)
                .Include(b => b.Nguoi.TinhThanh)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Danh sách bệnh nhân");

                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Mã bệnh nhân";
                worksheet.Cell(1, 3).Value = "Họ tên";
                worksheet.Cell(1, 4).Value = "Ngày sinh";
                worksheet.Cell(1, 5).Value = "Giới tính";
                worksheet.Cell(1, 6).Value = "Dân tộc";
                worksheet.Cell(1, 7).Value = "Tỉnh thành";
                worksheet.Cell(1, 8).Value = "Ngày nhập viện";
                worksheet.Cell(1, 9).Value = "Ngày xuất viện";
                worksheet.Cell(1, 10).Value = "Số ngày";
                worksheet.Cell(1, 11).Value = "Đơn giá (vnđ)";
                worksheet.Cell(1, 12).Value = "Tổng tiền (vnđ)";

                var culture = new CultureInfo("vi-VN");

                int row = 2;
                int stt = 1;
                foreach (var bn in data)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = bn.MaBenhNhan;
                    worksheet.Cell(row, 3).Value = bn.Nguoi?.HoTen;
                    worksheet.Cell(row, 4).Value = bn.Nguoi?.NgaySinh?.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 5).Value = bn.Nguoi?.GioiTinh;
                    worksheet.Cell(row, 6).Value = bn.Nguoi?.DanToc?.TenDanToc;
                    worksheet.Cell(row, 7).Value = bn.Nguoi?.TinhThanh?.TenTinh;
                    worksheet.Cell(row, 8).Value = bn.NgayNhapVien.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cell(row, 9).Value = bn.NgayXuatVien?.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 10).Value = bn.SoNgayNhapVien;
                    // 🟡 Định dạng tiền tệ theo kiểu "1.500.000"
                    decimal donGia = bn.DonGia ?? 0;
                    decimal tongTien = bn.TongTien ?? 0;

                    worksheet.Cell(row, 11).Value = donGia;
                    worksheet.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00"; // Hiển thị 2 chữ số thập phân

                    worksheet.Cell(row, 12).Value = tongTien;
                    worksheet.Cell(row, 12).Style.NumberFormat.Format = "#,##0.00"; // Cũng 2 chữ số thập phân
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    TempData["ExportSuccess"] = "Xuất Excel thành công!";
                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "DanhSachBenhNhan.xlsx");
                }
            }
        }


    }
}

