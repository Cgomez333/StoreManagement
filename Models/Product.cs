using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0.")]
        public float Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio.")]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public ICollection<InventoryMovement>? InventoryMovements { get; set; }
        public ICollection<SaleDetail>? SaleDetails { get; set; }
    }


}
