using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class ProductosController : Controller
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    
        return View(productos ?? new List<Producto>());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.CategoriaId = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nombre");
        return View(new Producto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        ModelState.Remove("Categoria");

        if (!ModelState.IsValid) 
        {
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();
        
        ViewBag.CategoriaId = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nombre", producto.CategoriaId);
        return View(producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();
        if (!ModelState.IsValid) 
        {
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (producto == null) return NotFound();
        return View(producto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            var tieneVentas = await _context.DetalleVentas.AnyAsync(d => d.ProductoId == id);
            if (tieneVentas)
            {
                ModelState.AddModelError("", "No se puede eliminar: el producto tiene registros de ventas.");
                return View(producto);
            }
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}