using System.ComponentModel.DataAnnotations;

namespace Subastas.Dto.Producto;

public class ProductoCreateRequest
{
    [Display(Name = "Nombre del producto")]
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    public string NombreProducto { get; set; } = null!;
    [Display(Name = "Descripción del producto")]
    [Required(ErrorMessage = "La descripción del producto es obligatorio")]
    [MaxLength(500)]
    public string DescripcionProducto { get; set; } = null!;
    [Display(Name = "Estado del producto")]
    public bool EstaActivo { get; set; } = true;
}