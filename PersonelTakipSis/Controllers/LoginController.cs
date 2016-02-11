using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PTS;

namespace PersonelTakipSis.Controllers
{
    public class LoginController : Controller
    {
        PetaPoco.Database db = new PetaPoco.Database("PTSConnection");
        //
        // GET: /Login/

        [HttpGet]
        public ActionResult Login()
        {
            return View(new User());
        }

        [HttpPost]
        public ActionResult Login(PTS.User user)
        {
            if (ModelState.IsValid)
            {               
                var getUser = db.SingleOrDefault<User>("Select *From [User] Where Username =@0 And Password=@1", user.Username, user.Password);
                if (getUser != null)
                {
                    var getRoles = db.Fetch<Role>("Select * from Role Inner Join UserRoles On Role.ID = UserRoles.RoleID Inner Join [User] On [User].ID = UserRoles.UserID Where [User].ID = @0 ", getUser.ID);
                    Session["User"] = getUser;
                    Session["Roles"] = getRoles;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(String.Empty, String.Empty);
                }
            }
            return View(user);
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

    }
}
