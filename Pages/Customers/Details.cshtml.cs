using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Hotel.Web.Pages.Customers
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public DetailsModel(HotelDbContext context) => _context = context;

        public CustomerDetailViewModel? Customer { get; set; }

        public async Task<IActionResult> OnGetAsync(string name)
        {
            var decodedName = HttpUtility.UrlDecode(name);

            var orders = await _context.StayOrders
                .Where(o => o.CustomerName == decodedName)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

            if (orders.Count == 0)
                return NotFound();

            Customer = new CustomerDetailViewModel
            {
                CustomerName = decodedName,
                CustomerPhone = orders.First().CustomerPhone,
                TotalStays = orders.Count,
                TotalSpending = orders.Sum(o => o.TotalAmount),
                Orders = orders
            };

            return Page();
        }
    }
}
