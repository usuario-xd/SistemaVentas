using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;

// 1) Cargar el archivo .env
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var linea in File.ReadAllLines(envFile))
    {
        var l = linea.Trim();
        if (l.Length == 0 || l.StartsWith('#') || !l.Contains('=')) continue;
        var partes = l.Split('=', 2);
        Environment.SetEnvironmentVariable(partes[0].Trim(), partes[1].Trim().Trim('"'));
    }
}

// Configurar cultura para usar punto decimal
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 2) Configuración de DbContext
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "ventas.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 3) Puerto para desplegar en Railway
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Habilitar página de errores detallados
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(); // Sirve archivos de la carpeta wwwroot
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();