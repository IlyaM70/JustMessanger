using Microsoft.AspNetCore.Mvc;


namespace MessagerWebClient.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
