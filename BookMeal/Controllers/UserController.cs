using BookMeal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WiseOffice;
using WiseOfficeSecurity;
using WiseOfficeSecurity.entities;
using System.Web.Routing;

namespace BookMeal.Controllers
{
    public class UserController : Controller
    {
        

        // GET: User
        public ActionResult Index()
        {
            USER_LOGIN userLogin = Session["USER_LOGIN"] as USER_LOGIN;
            if(userLogin != null) return Redirect("/home/");
            else return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            Session.Clear();
            string PasUserLogin = WiseOffice.clsSecuritys.Encrypt(password, "", true);
            EntitiesConnection dbContext = new EntitiesConnection();
            USER_LOGIN userLogin = dbContext.USER_LOGIN.FirstOrDefault(a => a.COD_USER_LOGIN == username && a.PAS_USER_LOGIN == PasUserLogin);
            if(userLogin != null)
            {
                Session["USER_LOGIN"] = userLogin;
                return Redirect("/home/");
            }
            else
            {
                TempData["ErrorMsg"] = "Tài khoản hoặc mật khẩu không đúng !";
                return Redirect("/user/");
            }
        }

        public ActionResult Logout()
        {
            Session["USER_LOGIN"] = null;
            Session.Clear();
            return Redirect("/user/");
        }
    }
}