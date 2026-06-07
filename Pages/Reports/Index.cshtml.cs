using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Pages.Reports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context) => _context = context;

        public RevenueSummaryViewModel Summary { get; set; } = new();
        public DailyReportViewModel? DailyReport { get; set; }
        public WeeklyReportViewModel? WeeklyReport { get; set; }
        public List<MonthlyReportViewModel> MonthlyReports { get; set; } = new();

        public DateTime ReportDate { get; set; } = DateTime.Today;
        public string ReportMode { get; set; } = "daily";  // daily / weekly / monthly
        public int WeekOffset { get; set; } = 0;

        public async Task OnGetAsync(DateTime? reportDate, string? mode, int? weekOffset)
        {
            if (reportDate.HasValue)
                ReportDate = reportDate.Value;
            if (!string.IsNullOrEmpty(mode))
                ReportMode = mode;
            if (weekOffset.HasValue)
                WeekOffset = weekOffset.Value;

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

            var completedOrders = await _context.StayOrders
                .Where(o => o.Status == "Completed")
                .ToListAsync();

            var todayCompleted = completedOrders
                .Where(o => o.ActualCheckOutTime >= today && o.ActualCheckOutTime < today.AddDays(1))
                .ToList();

            var todayCheckInsCount = await _context.StayOrders
                .CountAsync(o => o.ActualCheckInTime >= today && o.ActualCheckInTime < today.AddDays(1));

            var currentOccupied = await _context.Rooms.CountAsync(r => r.Status == "Occupied");
            var totalRooms = await _context.Rooms.CountAsync();

            Summary = new RevenueSummaryViewModel
            {
                TodayRevenue = todayCompleted.Sum(o => o.TotalAmount),
                ThisMonthRevenue = completedOrders
                    .Where(o => o.ActualCheckOutTime >= monthStart && o.ActualCheckOutTime < monthEnd.AddDays(1))
                    .Sum(o => o.TotalAmount),
                TotalRevenue = completedOrders.Sum(o => o.TotalAmount),
                TodayCheckIns = todayCheckInsCount,
                TodayCheckOuts = todayCompleted.Count,
                CurrentOccupied = currentOccupied,
                TotalRooms = totalRooms
            };

            // =========================
            // 日报
            // =========================
            if (ReportMode == "daily")
            {
                var dayStart = ReportDate;
                var dayEnd = dayStart.AddDays(1);

                var dayCompleted = completedOrders
                    .Where(o => o.ActualCheckOutTime >= dayStart && o.ActualCheckOutTime < dayEnd)
                    .ToList();

                var dayCheckIns = await _context.StayOrders
                    .CountAsync(o => o.ActualCheckInTime >= dayStart && o.ActualCheckInTime < dayEnd);

                var dayNewBookings = await _context.StayOrders
                    .CountAsync(o => o.CreatedTime >= dayStart && o.CreatedTime < dayEnd && o.Status == "Booked");

                DailyReport = new DailyReportViewModel
                {
                    Date = ReportDate,
                    CheckIns = dayCheckIns,
                    CheckOuts = dayCompleted.Count,
                    NewBookings = dayNewBookings,
                    RoomRevenue = dayCompleted.Sum(o => o.TotalAmount),
                    OccupiedRooms = currentOccupied,
                    TotalRooms = totalRooms
                };
            }

            // =========================
            // 周报
            // =========================
            if (ReportMode == "weekly")
            {
                var weekStart = ReportDate.AddDays(-(int)ReportDate.DayOfWeek + (int)DayOfWeek.Monday);
                if (ReportDate.DayOfWeek == DayOfWeek.Sunday)
                    weekStart = ReportDate.AddDays(-6);
                var weekEnd = weekStart.AddDays(7);

                WeeklyReport = new WeeklyReportViewModel
                {
                    WeekStart = weekStart,
                    WeekEnd = weekEnd.AddDays(-1)
                };

                for (var d = weekStart; d < weekEnd; d = d.AddDays(1))
                {
                    var dayCompleted = completedOrders
                        .Where(o => o.ActualCheckOutTime >= d && o.ActualCheckOutTime < d.AddDays(1))
                        .ToList();

                    var dayCheckIns = await _context.StayOrders
                        .CountAsync(o => o.ActualCheckInTime >= d && o.ActualCheckInTime < d.AddDays(1));

                    var dayNewBookings = await _context.StayOrders
                        .CountAsync(o => o.CreatedTime >= d && o.CreatedTime < d.AddDays(1) && o.Status == "Booked");

                    WeeklyReport.DailyDetails.Add(new DailyReportViewModel
                    {
                        Date = d,
                        CheckIns = dayCheckIns,
                        CheckOuts = dayCompleted.Count,
                        NewBookings = dayNewBookings,
                        RoomRevenue = dayCompleted.Sum(o => o.TotalAmount)
                    });
                }

                var weekCompleted = completedOrders
                    .Where(o => o.ActualCheckOutTime >= weekStart && o.ActualCheckOutTime < weekEnd)
                    .ToList();

                var roomTypes = await _context.Rooms
                    .ToDictionaryAsync(r => r.RoomNumber, r => r.RoomType);

                WeeklyReport.TotalRevenue = weekCompleted.Sum(o => o.TotalAmount);
                WeeklyReport.TotalOrders = weekCompleted.Count;
                WeeklyReport.TotalNights = weekCompleted.Sum(o => o.Nights);
                WeeklyReport.SingleRevenue = weekCompleted.Where(o => roomTypes.GetValueOrDefault(o.RoomNumber) == "Single").Sum(o => o.TotalAmount);
                WeeklyReport.DoubleRevenue = weekCompleted.Where(o => roomTypes.GetValueOrDefault(o.RoomNumber) == "Double").Sum(o => o.TotalAmount);
                WeeklyReport.SuiteRevenue = weekCompleted.Where(o => roomTypes.GetValueOrDefault(o.RoomNumber) == "Suite").Sum(o => o.TotalAmount);
            }

            // =========================
            // 月报
            // =========================
            var roomTypesMonthly = await _context.Rooms
                .ToDictionaryAsync(r => r.RoomNumber, r => r.RoomType);

            var sixMonthsAgo = today.AddMonths(-6);

            MonthlyReports = completedOrders
                .Where(o => o.ActualCheckOutTime >= sixMonthsAgo)
                .GroupBy(o => new { o.ActualCheckOutTime!.Value.Year, o.ActualCheckOutTime!.Value.Month })
                .Select(g => new MonthlyReportViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    TotalOrders = g.Count(),
                    TotalNights = g.Sum(o => o.Nights),
                    SingleRevenue = g.Where(o => roomTypesMonthly.GetValueOrDefault(o.RoomNumber) == "Single").Sum(o => o.TotalAmount),
                    DoubleRevenue = g.Where(o => roomTypesMonthly.GetValueOrDefault(o.RoomNumber) == "Double").Sum(o => o.TotalAmount),
                    SuiteRevenue = g.Where(o => roomTypesMonthly.GetValueOrDefault(o.RoomNumber) == "Suite").Sum(o => o.TotalAmount)
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();
        }
    }
}
