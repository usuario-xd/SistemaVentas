using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies; // <-- Necesario para el Login
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;

var builder = WebApplication.CreateBuilder(args);

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

// --- SERVICIOS ---
builder.Services.AddControllersWithViews();

// 2) Configuración de DbContext
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "ventas.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// --- NUEVO: Configuración de Autenticación por Cookies para el Login ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Ruta a la que redirige si no hay sesión
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Sesión de 8 horas (jornada laboral)
    });

// 3) Puerto para desplegar en Railway
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


// --- CONSTRUCCIÓN DE LA APP (Aquí está tu builder.Build) ---
var app = builder.Build();


// --- MIDDLEWARES (El orden es muy importante) ---
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

// NUEVO: Habilitar autenticación (siempre va antes de Authorization)
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();