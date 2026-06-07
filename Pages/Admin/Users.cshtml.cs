using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly HotelDbContext _context;

        public UsersModel(HotelDbContext context) => _context = context;

        public List<User> Users { get; set; } = new();
        public bool SaveSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _context.Users.OrderBy(u => u.Role).ThenBy(u => u.Username).ToListAsync();
        }

        public async Task<IActionResult> OnPostChangePwdAsync(int id, string newPwd)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new JsonResult(new { success = false });

            if (string.IsNullOrEmpty(newPwd) || newPwd.Length < 3)
                return new JsonResult(new { success = false });

            user.Password = newPwd;
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"修改账号 {user.Username} 密码",
                RoomNumber = "系统",
                Operator = User.Identity?.Name ?? "system",
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostAddUserAsync(string username, string password, int role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "用户名和密码不能为空";
                Users = await _context.Users.ToListAsync();
                return Page();
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ErrorMessage = "用户名已存在";
                Users = await _context.Users.ToListAsync();
                return Page();
            }

            _context.Users.Add(new User
            {
                Username = username,
                Password = password,
                Role = role
            });

            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"添加账号 {username}（{(role == 1 ? "管理员" : "前台")}）",
                RoomNumber = "系统",
                Operator = User.Identity?.Name ?? "system",
                Time = DateTime.Now
            });

            await _context.SaveChangesAsync();
            SaveSuccess = true;
            Users = await _context.Users.ToListAsync();
            return Page();
        }
    }
}
