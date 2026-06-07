namespace Hotel.Web.Models
{
    public class RecordViewModel
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string CustomerName { get; set; }
        public string Action { get; set; } // 预定、入住、退房
        public DateTime? ActionTime { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? Deposit { get; set; }
        public string Status { get; set; }
    }
}