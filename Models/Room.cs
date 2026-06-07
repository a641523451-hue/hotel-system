namespace Hotel.Web.Models
{
    public class Room
    {
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal Price { get; set; }
        public string CleanStatus { get; set; } = "Clean"; // Clean, Dirty
        public string Status { get; set; } = "Available"; // Available, Booked, Occupied
    }
}