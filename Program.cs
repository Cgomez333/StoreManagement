using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;
using System.Globalization;

namespace StoreManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configurar DbContext
            builder.Services.AddDbContext<StoreDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Política global que exige autenticación
            var requireAuthPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // 3. Añadir MVC con filtro global de autorización
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AuthorizeFilter(requireAuthPolicy));
            });

            // 4. Configurar cookie-based authentication
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opts =>
                {
                    opts.LoginPath = "/Account/Login";
                    opts.Cookie.Name = "StoreAuth";
                    opts.ExpireTimeSpan = TimeSpan.FromHours(2);
                });
            builder.Services.AddAuthorization();

            // 5. Cultura por defecto (es-CO)
            var culture = new CultureInfo("es-CO")
            {
                NumberFormat = { NumberDecimalSeparator = "." }
            };
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var app = builder.Build();

            // 6. Pipeline de middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // 7. Rutas
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
