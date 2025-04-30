namespace StoreManagement.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public float Total { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ICollection<SaleDetail> SaleDetails { get; set; }
    }

}
