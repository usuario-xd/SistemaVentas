using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class DetalleVentasController : Controller
{
    private readonly AppDbContext _context;

    public DetalleVentasController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var detalles = await _context.DetalleVentas
            .Include(d => d.Venta)
            .Include(d => d.Producto)
            .ToListAsync();
        return View(detalles);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.VentaId = new SelectList(await _context.Ventas.ToListAsync(), "Id", "Id");
        ViewBag.ProductoId = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre");
        return View(new DetalleVenta());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DetalleVenta detalleVenta)
    {
        // Limpiar propiedades de navegación para permitir una validación limpia de ModelState
        ModelState.Remove("Venta");
        ModelState.Remove("Producto");

        if (!ModelState.IsValid) 
        {
            ViewBag.VentaId = new SelectList(await _context.Ventas.ToListAsync(), "Id", "Id", detalleVenta.VentaId);
            ViewBag.ProductoId = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre", detalleVenta.ProductoId);
            return View(detalleVenta);
        }

        // Asignación directa a la propiedad Subtotal
        detalleVenta.Subtotal = detalleVenta.Cantidad * detalleVenta.Precio;
        
        _context.DetalleVentas.Add(detalleVenta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var detalle = await _context.DetalleVentas.FindAsync(id);
        if (detalle == null) return NotFound();
        
        ViewBag.VentaId = new SelectList(await _context.Ventas.ToListAsync(), "Id", "Id", detalle.VentaId);
        ViewBag.ProductoId = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre", detalle.ProductoId);
        return View(detalle);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DetalleVenta detalleVenta)
    {
        if (id != detalleVenta.Id) return NotFound();

        // Limpiar propiedades de navegación
        ModelState.Remove("Venta");
        ModelState.Remove("Producto");

        if (!ModelState.IsValid) 
        {
            ViewBag.VentaId = new SelectList(await _context.Ventas.ToListAsync(), "Id", "Id", detalleVenta.VentaId);
            ViewBag.ProductoId = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre", detalleVenta.ProductoId);
            return View(detalleVenta);
        }

        // Asignación directa a la propiedad Subtotal
        detalleVenta.Subtotal = detalleVenta.Cantidad * detalleVenta.Precio;

        _context.DetalleVentas.Update(detalleVenta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var detalle = await _context.DetalleVentas
            .Include(d => d.Producto)
            .Include(d => d.Venta)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        if (detalle == null) return NotFound();
        return View(detalle);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var detalle = await _context.DetalleVentas.FindAsync(id);
        if (detalle != null)
        {
            _context.DetalleVentas.Remove(detalle);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}