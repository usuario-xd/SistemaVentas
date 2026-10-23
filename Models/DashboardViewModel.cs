using SistemaVentas.Models;

namespace SistemaVentas.ViewModels
{
    public class DashboardViewModel
    {
        // Tarjetas superiores (KPIs)
        public decimal VentasHoy { get; set; }
        public decimal VentasMes { get; set; }
        public int TotalProductos { get; set; }
        public int TotalClientes { get; set; }
        public int ProductosBajoStock { get; set; }

        // Tablas
        public List<Venta> UltimasVentas { get; set; } = new();
        public List<Producto> ProductosCriticos { get; set; } = new();
    }
}