using Hotel.Web.Models;
using Hotel.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hotel.Web.Pages.Rooms
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly RoomService _roomService;

        public IndexModel(RoomService roomService)
        {
            _roomService = roomService;
        }

        public List<RoomViewModel> Rooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rooms = await _roomService.GetRoomListAsync();
        }

        public async Task<IActionResult> OnPostBookAsync(
            string roomNumber, string customerName, string customerPhone,
            DateTime checkInDate, DateTime checkOutDate, decimal price)
        {
            var result = await _roomService.BookAsync(
                roomNumber, customerName, customerPhone,
                checkInDate, checkOutDate, price, User.Identity?.Name ?? "system"
            );
            return new JsonResult(new { success = result });
        }

        public async Task<IActionResult> OnPostCheckInAsync(
            string roomNumber, string customerName, string customerPhone,
            decimal price, int nights, decimal prepaid)
        {
            var result = await _roomService.CheckInAsync(
                roomNumber, customerName, customerPhone,
                price, nights, prepaid, User.Identity?.Name ?? "system"
            );
            return new JsonResult(new { success = result });
        }

        public async Task<IActionResult> OnPostCheckOutAsync(string roomNumber)
        {
            var result = await _roomService.CheckOutAsync(roomNumber, User.Identity?.Name ?? "system");
            return new JsonResult(result ?? new { success = false });
        }

        public async Task<IActionResult> OnPostCancelBookingAsync(string roomNumber)
        {
            var result = await _roomService.CancelAsync(roomNumber, User.Identity?.Name ?? "system");
            return new JsonResult(new { success = result });
        }

        public async Task<IActionResult> OnPostMarkCleanAsync(string roomNumber)
        {
            var result = await _roomService.MarkCleanAsync(roomNumber, User.Identity?.Name ?? "system");
            return new JsonResult(new { success = result });
        }
    }
}
