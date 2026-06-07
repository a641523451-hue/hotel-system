using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hotel.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel.web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly HotelDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, HotelDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            // 测试数据库连接：尝试读取房间数量
            var roomCount = _context.Rooms.Count();
            ViewData["RoomCount"] = roomCount;
        }
    }
}
