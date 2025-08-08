using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
        public async Task<IActionResult> Add(BenhNhan model, int currentPage = 1, int pageSize = 5)
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await SaveBenhNhanAsync(model, isEdit: false);
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm bệnh nhân thành công!";
                    //return RedirectToAction("DanhSach", new { page = currentPage, pageSize = ViewBag.PageSize });
                    return RedirectToAction("DanhSach", new { page = currentPage, pageSize });
                }

                TempData["ErrorMessage"] = errorMessage ?? "Không thể thêm bệnh nhân";
                //return RedirectToAction("DanhSach", new { page = currentPage, pageSize = ViewBag.PageSize });
                return RedirectToAction("DanhSach", new { page = currentPage, pageSize });
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            //return RedirectToAction("DanhSach", new { page = currentPage, pageSize = ViewBag.PageSize });
            return RedirectToAction("DanhSach", new { page = currentPage, pageSize });
        }

        [HttpPost("BenhNhan/Edit/{MaBenhNhan}")]
        public async Task<IActionResult> Edit(string MaBenhNhan, BenhNhan model, int currentPage = 1, int pageSize = 5)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction("DanhSach", new { page = currentPage, pageSize });
            }

            var existingBenhNhan = await _dbService.BenhNhans
                .Include(bn => bn.Nguoi)
                .FirstOrDefaultAsync(bn => bn.MaBenhNhan == MaBenhNhan);

            if (existingBenhNhan == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bệnh nhân";
                return RedirectToAction("DanhSach", new { page = currentPage, pageSize });
            }

            var (success, errorMessage) = await SaveBenhNhanAsync(model, isEdit: true, existingEntity: existingBenhNhan);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật bệnh nhân thành công!";
                return RedirectToAction("DanhSach", new { page = currentPage, pageSize });
            }

            TempData["ErrorMessage"] = errorMessage ?? "Không thể cập nhật bệnh nhân";
            return RedirectToAction("DanhSach", new { page = currentPage, pageSize });
        }
        private async Task<(bool Success, string ErrorMessage)> SaveBenhNhanAsync(BenhNhan model, bool isEdit = false, BenhNhan existingEntity = null)
        {
            using var transaction = await _dbService.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra DanTocId hợp lệ
                var danTocExists = await _dbService.DanTocs
                    .AnyAsync(dt => dt.Id == model.Nguoi.DanTocId);
                if (!danTocExists)
                {
                    throw new Exception("Dân tộc không tồn tại. Vui lòng chọn lại.");
                }

                // Xử lý Tỉnh/Thành
                if (!string.IsNullOrEmpty(model.Nguoi.TinhThanh?.MaTinh))
                {
                    var existingTinhThanh = await _dbService.TinhThanhs
                        .FirstOrDefaultAsync(tt => tt.MaTinh == model.Nguoi.TinhThanh.MaTinh);

                    if (existingTinhThanh == null)
                    {
                        // Tạo mới
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

                    model.Nguoi.TinhThanh = null;
                }
                else
                {
                    model.Nguoi.TinhThanhId = null;
                }

                // Lưu mới hoặc cập nhật
                if (isEdit && existingEntity != null)
                {
                    // Cập nhật thông tin
                    existingEntity.Nguoi.HoTen = model.Nguoi.HoTen;
                    existingEntity.Nguoi.GioiTinh = model.Nguoi.GioiTinh;
                    existingEntity.Nguoi.NgaySinh = model.Nguoi.NgaySinh;
                    existingEntity.Nguoi.DanTocId = model.Nguoi.DanTocId;
                    existingEntity.Nguoi.TinhThanhId = model.Nguoi.TinhThanhId;

                    existingEntity.NgayNhapVien = model.NgayNhapVien;
                    existingEntity.NgayXuatVien = model.NgayXuatVien;
                    existingEntity.DonGia = model.DonGia;

                    if (model.NgayXuatVien.HasValue)
                    {
                        var ngayNhap = model.NgayNhapVien.Date;
                        var ngayXuat = model.NgayXuatVien.Value.Date;
                        existingEntity.SoNgayNhapVien = (ngayXuat - ngayNhap).Days + 1;
                        existingEntity.TongTien = existingEntity.SoNgayNhapVien * (model.DonGia ?? 0);
                    }

                    await _dbService.SaveChangesAsync();
                }
                else
                {
                    // Thêm mới
                    await _dbService.Nguois.AddAsync(model.Nguoi);
                    await _dbService.SaveChangesAsync();

                    model.MaNguoi = model.Nguoi.MaNguoi;
                    model.DonGia = model.DonGia ?? 0;

                    await _dbService.BenhNhans.AddAsync(model);
                    await _dbService.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi lưu bệnh nhân");
                return (false, ex.Message);
            }
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

                if (benhNhan == null) return NotFound();

                var danTocList = await _dbService.DanTocs.ToListAsync();
                ViewBag.DanTocList = danTocList;

                // Thêm logic xác định kiểu định dạng
                ViewBag.IsDecimal = benhNhan.DonGia % 1 != 0; // Kiểm tra có phần thập phân không
                return View(benhNhan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chỉnh sửa bệnh nhân");
                return StatusCode(500);
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
        // chỉnh lại trạng thái của bệnh nhân
        [HttpPost]
        public async Task<IActionResult> Delete(string id, int currentPage = 1, int pageSize = 5)
        {
            try
            {
                var benhNhan = await _dbService.BenhNhans
                    .Include(bn => bn.Nguoi)
                    .ThenInclude(n => n.TinhThanh) // Thêm ThenInclude nếu cần
                    .FirstOrDefaultAsync(bn => bn.MaBenhNhan == id);

                if (benhNhan == null)
                {
                    return NotFound();
                }

                // Thay vì xóa, cập nhật trạng thái Active
                if (benhNhan.Nguoi != null)
                {
                    // Cập nhật Active cho người
                    benhNhan.Active = false;

                    // Cập nhật Active cho tỉnh thành nếu cần
                    if (benhNhan.Nguoi.TinhThanh != null)
                    {
                        benhNhan.Nguoi.TinhThanh.Active = false;
                    }
                }

                // Cập nhật Active cho bệnh nhân
                benhNhan.Active = false;

                await _dbService.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã vô hiệu hóa bệnh nhân thành công!";

                return RedirectToAction("DanhSach", new
                {
                    page = currentPage > 0 ? currentPage : 1,
                    pageSize = pageSize > 0 ? pageSize : 5,
                    decimalFormat = TempData["DecimalFormat"]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi vô hiệu hóa bệnh nhân");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi vô hiệu hóa bệnh nhân";
                return RedirectToAction("DanhSach", new
                {
                    page = currentPage > 0 ? currentPage : 1,
                    pageSize = pageSize > 0 ? pageSize : 5
                });
            }
        }
        // Xóa bệnh nhân khỏi db
        //[HttpPost]
        //public async Task<IActionResult> Delete(string id, int currentPage = 1, int pageSize = 5)
        //{
        //    try
        //    {
        //        var benhNhan = await _dbService.BenhNhans
        //            .Include(bn => bn.Nguoi)
        //            .FirstOrDefaultAsync(bn => bn.MaBenhNhan == id);

        //        if (benhNhan == null)
        //        {
        //            return NotFound();
        //        }

        //        if (benhNhan.Nguoi != null)
        //        {
        //            _dbService.Nguois.Remove(benhNhan.Nguoi);
        //        }

        //        _dbService.BenhNhans.Remove(benhNhan);
        //        await _dbService.SaveChangesAsync();

        //        // Giữ nguyên trang hiện tại khi redirect
        //        return RedirectToAction("DanhSach", new
        //        {
        //            page = currentPage > 0 ? currentPage : 1,
        //            pageSize = pageSize > 0 ? pageSize : 5,
        //            decimalFormat = TempData["DecimalFormat"] // Giữ nguyên định dạng số
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi xóa bệnh nhân");
        //        return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bệnh nhân" });
        //    }
        //}


        //Đang query trực tiếp qua Entity Framework (EF Core) với .Include() và .ThenInclude()
        [HttpGet]
        public async Task<IActionResult> DanhSach(int page = 1, int pageSize = 5, bool decimalFormat = false)
        {
            var totalItems = await _dbService.BenhNhans.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            // đang dùng dạng entity framework
            //var danhSach = await _dbService.BenhNhans
            //    .Include(bn => bn.Nguoi)
            //    .ThenInclude(n => n.DanToc)
            //    .Include(bn => bn.Nguoi)
            //    .ThenInclude(n => n.TinhThanh)
            //    .Skip((page - 1) * pageSize)
            //    .Take(pageSize)
            //    .ToListAsync();

            // Sử dụng stored procedure để lấy danh sách bệnh nhân
            var danhSach = await _dbService.BenhNhanSTOs
                .FromSqlRaw("EXEC sp_GetBenhNhanPaged @PageNumber, @PageSize",
                new SqlParameter("@PageNumber", page),
                new SqlParameter("@PageSize", pageSize))
                .ToListAsync();

            // Lưu giá trị decimalFormat vào TempData để sử dụng khi redirect
            TempData["DecimalFormat"] = decimalFormat;

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
                // Sử dụng entity để lấy danh sách bệnh nhân
                //var data = await _dbService.BenhNhans
                //    .Include(b => b.Nguoi)
                //        .ThenInclude(n => n.DanToc)
                //    .Include(b => b.Nguoi)
                //        .ThenInclude(n => n.TinhThanh)
                //    .AsNoTracking()
                //    .ToListAsync();

                // Sử dụng stored procedure để lấy danh sách bệnh nhân
                var data = await _dbService.BenhNhanSTOs
                      .FromSqlRaw("EXEC sp_GetBenhNhan") // <-- Đổi tên proc nếu cần
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

        //[HttpGet]
        //public IActionResult ExportToExcel()
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            // Sử dụng entity để lấy danh sách bệnh nhân
            //var data = _dbService.BenhNhans
            //    .Include(b => b.Nguoi)
            //    .Include(b => b.Nguoi.DanToc)
            //    .Include(b => b.Nguoi.TinhThanh)
            //    .ToList();


            // Sử dụng stored procedure để lấy danh sách bệnh nhân
            var data = await _dbService.BenhNhanSTOs
          .FromSqlRaw("EXEC sp_GetBenhNhan") // <-- Đổi tên proc nếu cần
          .AsNoTracking()
          .ToListAsync();

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
                    worksheet.Cell(row, 3).Value = bn.HoTen;
                    worksheet.Cell(row, 4).Value = bn.NgaySinh?.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 5).Value = bn.GioiTinh;
                    worksheet.Cell(row, 6).Value = bn.TenDanToc;
                    worksheet.Cell(row, 7).Value = bn.TenTinh;
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
                //foreach (var bn in data)
                //{
                //    worksheet.Cell(row, 1).Value = stt++;
                //    worksheet.Cell(row, 2).Value = bn.MaBenhNhan;
                //    worksheet.Cell(row, 3).Value = bn.HoTen;
                //    worksheet.Cell(row, 4).Value = bn.Nguoi?.NgaySinh?.ToString("dd-MM-yyyy");
                //    worksheet.Cell(row, 5).Value = bn.Nguoi?.GioiTinh;
                //    worksheet.Cell(row, 6).Value = bn.Nguoi?.DanToc?.TenDanToc;
                //    worksheet.Cell(row, 7).Value = bn.Nguoi?.TinhThanh?.TenTinh;
                //    worksheet.Cell(row, 8).Value = bn.NgayNhapVien.ToString("dd-MM-yyyy HH:mm");
                //    worksheet.Cell(row, 9).Value = bn.NgayXuatVien?.ToString("dd-MM-yyyy");
                //    worksheet.Cell(row, 10).Value = bn.SoNgayNhapVien;
                //    // 🟡 Định dạng tiền tệ theo kiểu "1.500.000"
                //    decimal donGia = bn.DonGia ?? 0;
                //    decimal tongTien = bn.TongTien ?? 0;

                //    worksheet.Cell(row, 11).Value = donGia;
                //    worksheet.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00"; // Hiển thị 2 chữ số thập phân

                //    worksheet.Cell(row, 12).Value = tongTien;
                //    worksheet.Cell(row, 12).Style.NumberFormat.Format = "#,##0.00"; // Cũng 2 chữ số thập phân
                //    row++;
                //}

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

