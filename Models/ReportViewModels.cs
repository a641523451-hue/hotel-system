namespace Hotel.Web.Models
{
    public class DailyReportViewModel
    {
        public DateTime Date { get; set; }
        public int CheckIns { get; set; }
        public int CheckOuts { get; set; }
        public int NewBookings { get; set; }
        public decimal RoomRevenue { get; set; }
        public int OccupiedRooms { get; set; }
        public int TotalRooms { get; set; }
        public double OccupancyRate => TotalRooms > 0 ? Math.Round((double)OccupiedRooms / TotalRooms * 100, 1) : 0;
    }

    public class WeeklyReportViewModel
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalNights { get; set; }
        public decimal SingleRevenue { get; set; }
        public decimal DoubleRevenue { get; set; }
        public decimal SuiteRevenue { get; set; }

        public List<DailyReportViewModel> DailyDetails { get; set; } = new();
    }

    public class MonthlyReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalNights { get; set; }
        public decimal SingleRevenue { get; set; }
        public decimal DoubleRevenue { get; set; }
        public decimal SuiteRevenue { get; set; }
    }

    public class RevenueSummaryViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int CurrentOccupied { get; set; }
        public int TotalRooms { get; set; }
        public double OccupancyRate => TotalRooms > 0 ? Math.Round((double)CurrentOccupied / TotalRooms * 100, 1) : 0;
    }
}
