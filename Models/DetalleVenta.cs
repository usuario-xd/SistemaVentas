namespace SistemaVentas.Models;

public class DetalleVenta
{
    // Atributos
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }

    // Propiedad para almacenar o calcular el subtotal
    public decimal Subtotal { get; set; }

    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public void CalcularSubtotal()
    {
        Subtotal = Cantidad * Precio;
    }
}