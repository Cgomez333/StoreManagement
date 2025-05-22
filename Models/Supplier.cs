namespace StoreManagement.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Contact { get; set; }

        public ICollection<Product>? Products { get; set; }
    }

}
