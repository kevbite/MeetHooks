using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MeetHooks.Portal.Controllers
{
    [Authorize]
    public class SubscriptionsController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            var t1 = HttpContext.GetTokenAsync("access_token");
            var t2 = HttpContext.GetTokenAsync("refresh_token");
            return Content("Hello World");
        }
    }
}
