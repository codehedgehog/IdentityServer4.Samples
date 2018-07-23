using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MvcClient.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		[Authorize]
		public IActionResult Secure()
		{
			ViewData["Message"] = "Secure page.";

			return View();
		}

		public async Task Logout()
		{
			await HttpContext.SignOutAsync("Cookies");
			await HttpContext.SignOutAsync("oidc");
		}

		public IActionResult Error()
		{
			return View();
		}
	}
}