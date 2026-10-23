using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.ViewModels;

namespace SistemaVentas.Controllers
{
    [Authorize] // Protegido, solo usuarios logueados
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var viewModel = new DashboardViewModel
            {
                // 1. Ventas de hoy
                VentasHoy = await _context.Ventas
                    .Where(v => v.Fecha >= hoy)
                    .SumAsync(v => v.Total),

                // 2. Ventas del mes actual
                VentasMes = await _context.Ventas
                    .Where(v => v.Fecha >= inicioMes)
                    .SumAsync(v => v.Total),

                // 3. Totales generales
                TotalProductos = await _context.Productos.CountAsync(),
                TotalClientes = await _context.Clientes.CountAsync(),

                // 4. Productos con bajo stock (stock <= stock minimo)
                ProductosBajoStock = await _context.Productos
                    .CountAsync(p => p.Stock <= p.StockMinimo),

                // 5. Últimas 5 ventas
                UltimasVentas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .OrderByDescending(v => v.Fecha)
                    .Take(5)
                    .ToListAsync(),

                // 6. Productos críticos (los que necesitan reposición urgente)
                ProductosCriticos = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Stock <= p.StockMinimo)
                    .OrderBy(p => p.Stock)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}