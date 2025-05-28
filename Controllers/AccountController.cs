using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly PasswordHasher<User> _hasher;

        public AccountController(StoreDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // ¿Ya hay usuarios?
            bool anyUser = await _context.Users.AnyAsync();

            // Validar email único
            if (await _context.Users.AnyAsync(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("", "Ese correo ya está en uso.");
                return View(vm);
            }

            // Crear usuario y asignar rol
            var user = new User
            {
                Email = vm.Email,
                PasswordHash = _hasher.HashPassword(null!, vm.Password),
                Role = anyUser
                       ? "Administrador"    // si ya existía al menos uno
                       : "SuperAdmin"       // el primero
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == vm.Email);
            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password)
                  != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
