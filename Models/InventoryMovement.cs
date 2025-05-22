using System;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio.")]
        public int ProductId { get; set; }

        public Product? Product { get; set; } 

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
        public string Type { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        public string Reason { get; set; }
    }
}
