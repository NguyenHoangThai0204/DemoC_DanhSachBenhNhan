using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần thiết để dùng Include
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly Data.AppDbContext _dbService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(Data.AppDbContext dbService, ILogger<HomeController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachBenhNhan = await _dbService.BenhNhans
                .Include(bn => bn.Nguoi) // <-- nạp kèm quan hệ Nguoi
                .ToListAsync();

            return View(danhSachBenhNhan);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




    }
}



//public class HomeController : Controller
//{
//    private readonly Data.AppDbContext _dbService;

//public HomeController(Data.AppDbContext dbService)
//{
//    _dbService = dbService;
//}

//    public IActionResult Index()
//    {
//        bool isConnected = _dbService.TestConnection();
//        ViewBag.DbStatus = isConnected ? "✅ Kết nối thành công" : "❌ Không thể kết nối DB";
//        return View();
//    }
//}