using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Pages.Folio
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public List<StayOrder> Orders { get; set; } = new();
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync(string? keyword, string? statusFilter)
        {
            Keyword = keyword;
            StatusFilter = statusFilter;

            var query = _context.StayOrders.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o =>
                    o.RoomNumber.Contains(keyword) ||
                    o.CustomerName.Contains(keyword) ||
                    o.CustomerPhone.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            Orders = await query
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();
        }
    }
}
