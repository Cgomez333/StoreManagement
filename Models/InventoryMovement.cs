namespace StoreManagement.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime Date { get; set; }

        public string Type { get; set; }  // e.g., "IN" or "OUT"

        public int Quantity { get; set; }

        public string Reason { get; set; }
    }

}
