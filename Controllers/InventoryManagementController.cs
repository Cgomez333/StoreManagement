using Microsoft.AspNetCore.Mvc;

namespace StoreManagement.Controllers
{
    public class InventoryManagementController : Controller
    {
        // GET: /InventoryManagement/
        public IActionResult Index()
        {
            return View();
        }

        // Aquí puedes agregar más acciones en el futuro como:
        // public IActionResult Create() { }
        // public IActionResult Edit(int id) { }
        // public IActionResult Delete(int id) { }
        // etc.
    }
}
