using Hotel.Web.Data;
using Hotel.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Services
// =========================
builder.Services.AddRazorPages();

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<RoomService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login?handler=Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// =========================
// Seed database
// =========================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

    db.Database.EnsureCreated();

    // Seed rooms
    if (!db.Rooms.Any())
    {
        var rooms = new List<Hotel.Web.Models.Room>();

        for (int i = 101; i <= 112; i++)
        {
            rooms.Add(new Hotel.Web.Models.Room
            {
                RoomNumber = i.ToString(),
                RoomType = (i <= 106) ? "Single" : "Double",
                Price = (i <= 106) ? 200 : 300,
                CleanStatus = "Clean",
                Status = "Available"
            });
        }

        for (int i = 201; i <= 212; i++)
        {
            rooms.Add(new Hotel.Web.Models.Room
            {
                RoomNumber = i.ToString(),
                RoomType = (i <= 206) ? "Single" : "Double",
                Price = (i <= 206) ? 220 : 320,
                CleanStatus = "Clean",
                Status = "Available"
            });
        }

        for (int i = 301; i <= 305; i++)
        {
            rooms.Add(new Hotel.Web.Models.Room
            {
                RoomNumber = i.ToString(),
                RoomType = "Suite",
                Price = 600,
                CleanStatus = "Clean",
                Status = "Available"
            });
        }

        db.Rooms.AddRange(rooms);
        db.SaveChanges();
    }

    // Seed default admin user
    if (!db.Users.Any())
    {
        db.Users.Add(new Hotel.Web.Models.User
        {
            Username = "admin",
            Password = "admin123",
            Role = 1
        });
        db.Users.Add(new Hotel.Web.Models.User
        {
            Username = "front",
            Password = "front123",
            Role = 0
        });
        db.SaveChanges();
    }
}

// =========================
// Middleware
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
