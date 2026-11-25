using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pedidos360.Controllers
{
    // RF-06: solo usuarios con rol Admin o Vendedor pueden entrar a la gestion de ordenes.
    [Authorize(Roles = "Admin,Vendedor")]
    public class OrdenesController : Controller
    {
        // RF-03: la pantalla no recibe datos del servidor; los pide por AJAX a /api/ordenes.
        public IActionResult Index()
        {
            return View();
        }
    }
}
