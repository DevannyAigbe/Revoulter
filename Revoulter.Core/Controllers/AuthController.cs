using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Revoulter.Core.Controllers
{
    public class AuthController : Controller
    {
        [AllowAnonymous]
        public IActionResult UserLogin()
        {
            return View();
        }
    }
}
