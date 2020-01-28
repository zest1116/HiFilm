using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            
            var users = await GraphHelper.GetUsersAsync().ConfigureAwait(false);
            return View(users);
        }

        public async Task<ActionResult> Create()
        {
            User user = await GraphHelper.CreateUserAsync().ConfigureAwait(false);
            return RedirectToAction("Index", "Users");
        }
    }
}