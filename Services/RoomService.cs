using Hotel.Web.Data;
using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Services
{
    public class RoomService
    {
        private readonly HotelDbContext _context;
        public RoomService(HotelDbContext context) => _context = context;

        public async Task<List<RoomViewModel>> GetRoomListAsync()
        {
            var rooms = await _context.Rooms.ToListAsync();

            return rooms.Select(r => new RoomViewModel
            {
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Price = r.Price,
                Status = r.Status,
                CleanStatus = r.CleanStatus,

                StatusDisplay = r.Status switch
                {
                    "Available" => "空闲",
                    "Booked" => "已预订",
                    "Occupied" => "入住中",
                    _ => "未知"
                },

                CleanStatusDisplay = r.CleanStatus switch
                {
                    "Clean" => "干净",
                    "Dirty" => "待清扫",
                    _ => "干净"
                },

                TypeDisplay = r.RoomType switch
                {
                    "Single" => "单间",
                    "Double" => "标间",
                    "Suite" => "套房",
                    _ => r.RoomType
                }
            }).ToList();
        }

        // =========================
        // 预订
        // =========================
        public async Task<bool> BookAsync(string roomNumber, string name, string phone,
            DateTime checkIn, DateTime checkOut, decimal price, string operatorName)
        {
            var room = await _context.Rooms.FindAsync(roomNumber);
            if (room == null || room.Status != "Available") return false;

            int nights = Math.Max(1, (checkOut - checkIn).Days);

            var order = new StayOrder
            {
                RoomNumber = roomNumber,
                CustomerName = name,
                CustomerPhone = phone,
                PromisedCheckIn = checkIn,
                PromisedCheckOut = checkOut,
                UnitPrice = price,
                Nights = nights,
                TotalAmount = price * nights,
                PaidAmount = 0,
                OutstandingAmount = price * nights,
                Status = "Booked",
                CreatedTime = DateTime.Now
            };

            room.Status = "Booked";
            _context.StayOrders.Add(order);
            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"预订房间 {roomNumber} - {name}",
                RoomNumber = roomNumber,
                Operator = operatorName,
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // 入住（预订转入住 / 直接入住）
        // =========================
        public async Task<bool> CheckInAsync(string roomNumber, string name, string phone,
            decimal unitPrice, int nights, decimal prepaid, string operatorName = "system")
        {
            var room = await _context.Rooms.FindAsync(roomNumber);
            if (room == null) return false;

            var order = await _context.StayOrders
                .FirstOrDefaultAsync(o => o.RoomNumber == roomNumber && o.Status == "Booked");

            if (order != null)
            {
                // 预订转入住：更新信息
                order.CustomerName = name;
                order.CustomerPhone = phone;
                order.UnitPrice = unitPrice;
                order.Nights = nights;
                order.TotalAmount = unitPrice * nights;
                order.PaidAmount = prepaid;
                order.OutstandingAmount = order.TotalAmount - prepaid;
                order.ActualCheckInTime = DateTime.Now;
                order.Status = "CheckedIn";
            }
            else
            {
                // 直接入住（Walk-in）
                _context.StayOrders.Add(new StayOrder
                {
                    RoomNumber = roomNumber,
                    CustomerName = name,
                    CustomerPhone = phone,
                    Status = "CheckedIn",
                    ActualCheckInTime = DateTime.Now,
                    UnitPrice = unitPrice,
                    Nights = nights,
                    TotalAmount = unitPrice * nights,
                    PaidAmount = prepaid,
                    OutstandingAmount = (unitPrice * nights) - prepaid,
                    PromisedCheckIn = DateTime.Today,
                    PromisedCheckOut = DateTime.Today.AddDays(nights),
                    CreatedTime = DateTime.Now
                });
            }

            room.Status = "Occupied";
            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"入住房间 {roomNumber} - {name}，预付 ¥{prepaid:F2}",
                RoomNumber = roomNumber,
                Operator = operatorName,
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // 退房（返回账单 JSON）
        // =========================
        public async Task<object?> CheckOutAsync(string roomNumber, string operatorName = "system")
        {
            var room = await _context.Rooms.FindAsync(roomNumber);
            var order = await _context.StayOrders
                .FirstOrDefaultAsync(o => o.RoomNumber == roomNumber && o.Status == "CheckedIn");

            if (room == null || order == null) return null;

            // 实际入住天数（最少1天）
            var actualCheckIn = order.ActualCheckInTime ?? order.PromisedCheckIn;
            var actualNights = Math.Max(1, (DateTime.Now.Date - actualCheckIn.Date).Days);

            var totalAmount = order.UnitPrice * actualNights;
            var paidAmount = order.PaidAmount;
            var outstandingAmount = totalAmount - paidAmount;

            order.Nights = actualNights;
            order.TotalAmount = totalAmount;
            order.OutstandingAmount = outstandingAmount;
            order.Status = "Completed";
            order.ActualCheckOutTime = DateTime.Now;

            room.Status = "Available";
            room.CleanStatus = "Dirty";

            // 记录预付金为Payment（如果还没记录过，且预付金>0）
            if (paidAmount > 0)
            {
                var hasPaymentRecord = await _context.Payments
                    .AnyAsync(p => p.StayOrderId == order.Id && p.Remark == "预付金");
                if (!hasPaymentRecord)
                {
                    _context.Payments.Add(new Payment
                    {
                        StayOrderId = order.Id,
                        Amount = paidAmount,
                        Method = "Cash",
                        Remark = "预付金",
                        PaidTime = order.ActualCheckInTime ?? DateTime.Now
                    });
                }
            }

            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"退房房间 {roomNumber} - {order.CustomerName}，总价 ¥{totalAmount:F2}，已付 ¥{paidAmount:F2}，欠款 ¥{outstandingAmount:F2}",
                RoomNumber = roomNumber,
                Operator = operatorName,
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // 返回账单数据
            return new
            {
                success = true,
                bill = new
                {
                    roomNumber = order.RoomNumber,
                    customerName = order.CustomerName,
                    customerPhone = order.CustomerPhone,
                    checkIn = actualCheckIn.ToString("yyyy-MM-dd HH:mm"),
                    checkOut = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    unitPrice = order.UnitPrice,
                    nights = actualNights,
                    totalAmount = totalAmount,
                    paidAmount = paidAmount,
                    outstandingAmount = outstandingAmount
                }
            };
        }

        // =========================
        // 取消预订
        // =========================
        public async Task<bool> CancelAsync(string roomNumber, string operatorName = "system")
        {
            var room = await _context.Rooms.FindAsync(roomNumber);
            var order = await _context.StayOrders
                .FirstOrDefaultAsync(o => o.RoomNumber == roomNumber && o.Status == "Booked");

            if (room == null || order == null) return false;

            order.Status = "Cancelled";
            room.Status = "Available";

            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"取消预订房间 {roomNumber}",
                RoomNumber = roomNumber,
                Operator = operatorName,
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // 标记清洁
        // =========================
        public async Task<bool> MarkCleanAsync(string roomNumber, string operatorName = "system")
        {
            var room = await _context.Rooms.FindAsync(roomNumber);
            if (room == null || room.CleanStatus != "Dirty") return false;

            room.CleanStatus = "Clean";

            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"标记房间 {roomNumber} 已清洁",
                RoomNumber = roomNumber,
                Operator = operatorName,
                Time = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
