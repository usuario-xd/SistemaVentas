using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.ViewModels;

namespace SistemaVentas.Controllers
{
    [Authorize] // Protegido para que solo entren los logueados
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, int? clienteId)
        {
            // 1. Establecer fechas por defecto (inicio de mes hasta hoy)
            var fechaDesde = desde ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fechaHasta = hasta ?? DateTime.Now;

            // Asegurarnos de que la fecha hasta incluya todo el día
            var fechaHastaFinDia = fechaHasta.Date.AddDays(1).AddTicks(-1);

            var viewModel = new ReportesViewModel
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                ClienteId = clienteId,
                TodosLosClientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync()
            };

            // 2. Consulta base de ventas en el rango de fechas
            var queryVentas = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Where(v => v.Fecha >= fechaDesde && v.Fecha <= fechaHastaFinDia);

            if (clienteId.HasValue && clienteId > 0)
            {
                queryVentas = queryVentas.Where(v => v.ClienteId == clienteId.Value);
            }

            var ventas = await queryVentas.OrderByDescending(v => v.Fecha).ToListAsync();

            // 3. Calcular Resumen
            viewModel.VentasDelPeriodo = ventas;
            viewModel.VentasTotales = ventas.Sum(v => v.Total); // Asumo que tu modelo Venta tiene propiedad Total
            viewModel.CantidadTransacciones = ventas.Count;
            viewModel.TicketPromedio = ventas.Count > 0 ? viewModel.VentasTotales / ventas.Count : 0;

            // 4. Calcular Ventas por Producto y Producto Estrella
            var detalles = ventas.SelectMany(v => v.Detalles).ToList();
            
            var productosAgrupados = detalles
                .GroupBy(d => new { d.ProductoId, d.Producto.Nombre, Categoria = d.Producto.Categoria.Nombre })
                .Select(g => new ProductoVendido
                {
                    Nombre = g.Key.Nombre,
                    Categoria = g.Key.Categoria,
                    Unidades = g.Sum(d => d.Cantidad),
                    MontoTotal = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.Unidades)
                .ToList();

            viewModel.VentasPorProducto = productosAgrupados;

            var estrella = productosAgrupados.FirstOrDefault();
            if (estrella != null)
            {
                viewModel.ProductoEstrella = estrella.Nombre;
                viewModel.UnidadesProductoEstrella = estrella.Unidades;
            }

            // 5. Stock y Clientes
            viewModel.StockProductos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            viewModel.ListadoClientes = await _context.Clientes
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(viewModel);
        }
    }
}