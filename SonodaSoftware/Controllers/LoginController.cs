using Microsoft.AspNetCore.Mvc;
using SonodaSoftware.Data;
using SonodaSoftware.Services;

namespace SonodaSoftware.Controllers
{
    public class LoginController : Controller
    {
        SND_DBContext _userContext = new SND_DBContext();
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LoginByUser(User_UserBase sndUser)
        {
            if (sndUser == null || string.IsNullOrEmpty(sndUser.Username) || string.IsNullOrEmpty(sndUser.Password))
            {
                TempData["ErrorLogin"] = "Username or Password cannot be empty.";
                return RedirectToAction("Login", "Home");
            }

            var UserCheck = _userContext.User_UserBases.FirstOrDefault(x => x.Username == sndUser.Username);

            if (UserCheck == null || sndUser.Password != UserCheck.Password)
            {
                TempData["ErrorLogin"] = "Invalid username or password.";
                return RedirectToAction("Login", "Home");
            }

            HttpContext.Session.Set<User_UserBase>("UserLogin", UserCheck);
            return RedirectToAction("Index", "Home");
        }
    }
}
