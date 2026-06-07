namespace Hotel.Web.Models
{
    public class CustomerSummaryViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int TotalStays { get; set; }
        public decimal TotalSpending { get; set; }
        public DateTime? LastStay { get; set; }
    }

    public class CustomerDetailViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int TotalStays { get; set; }
        public decimal TotalSpending { get; set; }
        public List<StayOrder> Orders { get; set; } = new();
    }
}
