using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Pages.Customers
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context) => _context = context;

        public List<CustomerSummaryViewModel> Customers { get; set; } = new();
        public string? Keyword { get; set; }

        public async Task OnGetAsync(string? keyword)
        {
            Keyword = keyword;

            var query = _context.StayOrders
                .Where(o => o.Status == "Completed" || o.Status == "CheckedIn")
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o =>
                    o.CustomerName.Contains(keyword) || o.CustomerPhone.Contains(keyword));
            }

            Customers = await query
                .GroupBy(o => new { o.CustomerName, o.CustomerPhone })
                .Select(g => new CustomerSummaryViewModel
                {
                    CustomerName = g.Key.CustomerName,
                    CustomerPhone = g.Key.CustomerPhone,
                    TotalStays = g.Count(),
                    TotalSpending = g.Sum(o => o.TotalAmount),
                    LastStay = g.Max(o => o.ActualCheckInTime)
                })
                .OrderByDescending(c => c.TotalSpending)
                .ToListAsync();
        }
    }
}
