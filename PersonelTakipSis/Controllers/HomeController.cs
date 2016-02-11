using PTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace PersonelTakipSis.Controllers
{
    public class HomeController : Controller
    {

        //
        // GET: /Home/
        public ActionResult Index()
        {
            if (Session["User"] as User != null)
            {
                User user = Session["User"] as User;
                ViewBag.Username = user.Username;
                return View();
            }
            return RedirectToAction("Login", "Login");

        }
    }
}
