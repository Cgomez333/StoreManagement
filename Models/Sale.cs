namespace StoreManagement.Models;

public class Sale
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    // Hacer setter privado para evitar que se modifique directamente
    public float Total { get; private set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public List<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    // Método para recalcular Total basado en SaleDetails
    public void RecalculateTotal()
    {
        Total = SaleDetails?.Sum(sd => sd.Subtotal) ?? 0;
    }
}
