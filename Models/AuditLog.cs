namespace Hotel.Web.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string Action { get; set; }

        public string RoomNumber { get; set; }

        public string? Operator { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;
    }
}