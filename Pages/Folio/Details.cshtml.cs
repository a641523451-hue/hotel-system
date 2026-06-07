using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Pages.Folio
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public DetailsModel(HotelDbContext context)
        {
            _context = context;
        }

        public StayOrder Order { get; set; } = null!;
        public List<Payment> Payments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.StayOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (Order == null) return NotFound();

            Payments = await _context.Payments
                .Where(p => p.StayOrderId == id)
                .OrderByDescending(p => p.PaidTime)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostPayAsync(int id, decimal amount, string method, string? remark)
        {
            var order = await _context.StayOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (string.IsNullOrEmpty(method)) method = "Cash";

            _context.Payments.Add(new Payment
            {
                StayOrderId = id,
                Amount = amount,
                Method = method,
                Remark = remark,
                PaidTime = DateTime.Now
            });

            await _context.SaveChangesAsync();

            var paid = await _context.Payments
                .Where(p => p.StayOrderId == id)
                .SumAsync(p => p.Amount);

            order.PaidAmount = paid;
            order.OutstandingAmount = order.TotalAmount - paid;

            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"收取订单 #{id} 金额 ¥{amount:F2}，方式：{method}",
                RoomNumber = order.RoomNumber,
                Operator = User.Identity?.Name ?? "system",
                Time = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }
    }
}
