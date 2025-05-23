using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    public class DashboardController : Controller
    {
        private readonly StoreDbContext _context;
        public DashboardController(StoreDbContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> MtdSalesByDay()
        {
            var nowUtc = DateTime.UtcNow;
            var firstDayThisMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstDayNextMonth = firstDayThisMonth.AddMonths(1);

            var data = await _context.Sales
                .Where(s => s.Date >= firstDayThisMonth && s.Date < firstDayNextMonth)
                .GroupBy(s => s.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    amount = g.Sum(s => s.Total)
                })
                .ToListAsync();

            return Json(data);
        }


        [HttpGet]
        public async Task<IActionResult> SalesByProduct()
        {
            var data = await _context.SaleDetails
                .Include(d => d.Product)
                .GroupBy(d => d.Product.Name)
                .Select(g => new {
                    product  = g.Key,
                    subtotal = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.subtotal)
                .Take(10)
                .ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> TopCustomers()
        {
            var data = await _context.Sales
                .Include(s => s.Customer)
                .GroupBy(s => s.Customer.Name)
                .Select(g => new {
                    customer = g.Key,
                    total    = g.Sum(s => s.Total)
                })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> AverageProductValue()
        {
            var avg = await _context.Products.AverageAsync(p => p.Price);
            return Json(new { avg });
        }

        // --- FILTROS DE MES ANTERIOR ---

        [HttpGet]
        public async Task<IActionResult> PreviousMonthRevenue()
        {
            var nowUtc = DateTime.UtcNow;
            // inicio del mes actual en UTC
            var firstDayThisMonthUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            // inicio del mes anterior
            var firstDayPrevUtc = firstDayThisMonthUtc.AddMonths(-1);

            var totalPrev = await _context.Sales
                .Where(s => s.Date >= firstDayPrevUtc && s.Date < firstDayThisMonthUtc)
                .SumAsync(s => s.Total);

            return Json(new { totalPrev });
        }

        [HttpGet]
        public async Task<IActionResult> PreviousMonthOrdersCount()
        {
            var nowUtc = DateTime.UtcNow;
            var firstDayThisMonthUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstDayPrevUtc = firstDayThisMonthUtc.AddMonths(-1);

            var countPrev = await _context.Sales
                .CountAsync(s => s.Date >= firstDayPrevUtc && s.Date < firstDayThisMonthUtc);

            return Json(new { countPrev });
        }

        [HttpGet]
        public async Task<IActionResult> PreviousAvgOrderValue()
        {
            var nowUtc = DateTime.UtcNow;
            var firstDayThisMonthUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstDayPrevUtc = firstDayThisMonthUtc.AddMonths(-1);

            // traemos totales del mes anterior
            var list = await _context.Sales
                .Where(s => s.Date >= firstDayPrevUtc && s.Date < firstDayThisMonthUtc)
                .Select(s => s.Total)
                .ToListAsync();

            // calcular promedio (evitando división por cero)
            var avgPrev = list.Any()
                ? list.Average()
                : 0f;

            return Json(new { avgPrev });
        }
    }
}
