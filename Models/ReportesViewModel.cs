using SistemaVentas.Models;

namespace SistemaVentas.ViewModels
{
    public class ReportesViewModel
    {
        // Filtros
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int? ClienteId { get; set; }

        // Resumen del periodo
        public decimal VentasTotales { get; set; }
        public int CantidadTransacciones { get; set; }
        public decimal TicketPromedio { get; set; }
        public string ProductoEstrella { get; set; } = "N/A";
        public int UnidadesProductoEstrella { get; set; }

        // Listas para las tablas
        public List<Venta> VentasDelPeriodo { get; set; } = new();
        public List<ProductoVendido> VentasPorProducto { get; set; } = new();
        public List<Producto> StockProductos { get; set; } = new();
        public List<Cliente> ListadoClientes { get; set; } = new();

        // Lista de clientes para el dropdown del filtro
        public List<Cliente> TodosLosClientes { get; set; } = new();
    }

    // Clase auxiliar para la tabla de "Ventas por producto"
    public class ProductoVendido
    {
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int Unidades { get; set; }
        public decimal MontoTotal { get; set; }
    }
}