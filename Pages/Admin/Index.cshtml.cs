using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context) => _context = context;

        public List<Room> Rooms { get; set; } = new();
        public bool SaveSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Rooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(List<Room> rooms)
        {
            if (rooms == null || rooms.Count == 0)
                return RedirectToPage();

            foreach (var updated in rooms)
            {
                var room = await _context.Rooms.FindAsync(updated.RoomNumber);
                if (room != null)
                {
                    room.RoomType = updated.RoomType;
                    room.Price = updated.Price;
                }
            }

            await _context.SaveChangesAsync();
            SaveSuccess = true;
            Rooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteRoomAsync(string roomNumber)
        {
            var room = await _context.Rooms.FindAsync(roomNumber);
            if (room == null)
            {
                ErrorMessage = $"房间 {roomNumber} 不存在";
                Rooms = await _context.Rooms.ToListAsync();
                return Page();
            }

            if (room.Status != "Available")
            {
                ErrorMessage = $"房间 {roomNumber} 当前不是空闲状态，无法删除";
                Rooms = await _context.Rooms.ToListAsync();
                return Page();
            }

            _context.Rooms.Remove(room);
            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"删除房间 {roomNumber}",
                RoomNumber = roomNumber,
                Operator = User.Identity?.Name ?? "system",
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();

            SaveSuccess = true;
            Rooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddRoomAsync(string roomNumber, string roomType, decimal price)
        {
            if (string.IsNullOrEmpty(roomNumber))
            {
                ErrorMessage = "请输入房号";
                Rooms = await _context.Rooms.ToListAsync();
                return Page();
            }

            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == roomNumber))
            {
                ErrorMessage = $"房间 {roomNumber} 已存在";
                Rooms = await _context.Rooms.ToListAsync();
                return Page();
            }

            _context.Rooms.Add(new Room
            {
                RoomNumber = roomNumber,
                RoomType = roomType,
                Price = price,
                Status = "Available",
                CleanStatus = "Clean"
            });

            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"添加房间 {roomNumber}（{roomType}，¥{price}）",
                RoomNumber = roomNumber,
                Operator = User.Identity?.Name ?? "system",
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();

            SaveSuccess = true;
            Rooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
            return Page();
        }
    }
}
