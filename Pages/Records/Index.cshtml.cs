using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hotel.Web.Data;
using Hotel.Web.Models;

namespace Hotel.Web.Pages.Records
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public List<AuditLog> AuditLogs { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? RoomFilter { get; set; }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate, string? roomFilter)
        {
            StartDate = startDate ?? DateTime.Today.AddDays(-7);
            EndDate = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);
            RoomFilter = roomFilter;

            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.Time >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Time <= endDate.Value);

            if (!string.IsNullOrEmpty(roomFilter))
                query = query.Where(l => l.RoomNumber.Contains(roomFilter));

            AuditLogs = await query
                .OrderByDescending(l => l.Time)
                .Take(500)
                .ToListAsync();
        }
    }
}
