using AiYi.Pay.IPayment;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Func<string, AiYi.Pay.IPayment.IPay> serviceAccessor;
        public HomeController(Func<string, AiYi.Pay.IPayment.IPay> serviceAccessor,ILogger<HomeController> logger)
        {
            this.serviceAccessor = serviceAccessor;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var service = serviceAccessor("AiYi.Pay.WeiXin");
            var payment = new AiYi.Pay.Config();
            var dr = service.CallbackUrl(payment);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
