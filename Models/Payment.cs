namespace Hotel.Web.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int StayOrderId { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } // Cash / Card / Alipay

        public DateTime PaidTime { get; set; } = DateTime.Now;

        public string? Remark { get; set; }
    }
}