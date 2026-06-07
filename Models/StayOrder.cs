namespace Hotel.Web.Models
{
    public class StayOrder
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime PromisedCheckIn { get; set; }
        public DateTime PromisedCheckOut { get; set; }
        public DateTime? ActualCheckInTime { get; set; }
        public DateTime? ActualCheckOutTime { get; set; }

        public int Nights { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public decimal OutstandingAmount { get; set; }

        public string Status { get; set; }
    }
}
