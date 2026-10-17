using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class VentasController : Controller
{
    private readonly AppDbContext _context;

    public VentasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Ventas
    public async Task<IActionResult> Index()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return View(ventas ?? new List<Venta>());
    }

    // GET: Ventas/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        return View(venta);
    }

    // GET: Ventas/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Clientes = await _context.Clientes.ToListAsync();
        ViewBag.Productos = await _context.Productos.ToListAsync();
        return View(new Venta());
    }

    // POST: Ventas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Venta venta)
    {
        // Se remueven propiedades de navegación para evitar falsos errores de model state
        ModelState.Remove("Cliente");
        foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Detalles")).ToList())
        {
            if (key.EndsWith(".Producto") || key.EndsWith(".Venta"))
            {
                ModelState.Remove(key);
            }
        }

        if (venta.Detalles == null || !venta.Detalles.Any())
        {
            ModelState.AddModelError("", "Debe agregar al menos un producto a la venta.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ClienteId = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre", venta.ClienteId);
            ViewBag.Productos = await _context.Productos.ToListAsync();
            return View(venta);
        }

        foreach (var detalle in venta.Detalles)
        {
            detalle.Subtotal = detalle.Cantidad * detalle.Precio;
        }

        venta.CalcularTotal();
        venta.CalcularVambio();

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Ventas/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        ViewBag.ClienteId = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre", venta.ClienteId);
        ViewBag.Productos = await _context.Productos.ToListAsync();

        return View(venta);
    }

    // POST: Ventas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Venta venta)
    {
        if (id != venta.Id) return NotFound();

        ModelState.Remove("Cliente");
        foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Detalles")).ToList())
        {
            if (key.EndsWith(".Producto") || key.EndsWith(".Venta"))
            {
                ModelState.Remove(key);
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ClienteId = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre", venta.ClienteId);
            ViewBag.Productos = await _context.Productos.ToListAsync();
            return View(venta);
        }

        foreach (var detalle in venta.Detalles)
        {
            detalle.Subtotal = detalle.Cantidad * detalle.Precio;
        }

        venta.CalcularTotal();
        venta.CalcularVambio();

        _context.Ventas.Update(venta);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Ventas/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        return View(venta);
    }

    // POST: Ventas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta != null)
        {
            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}