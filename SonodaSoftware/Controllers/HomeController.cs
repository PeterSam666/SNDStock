using Microsoft.AspNetCore.Mvc;
using SonodaSoftware.Models;
using System.Diagnostics;
using SonodaSoftware.Services;
using SonodaSoftware.Data;

namespace SonodaSoftware.Controllers
{
    [SessionFilter(AllowAction = new[] { "Login", "loginCheck", "LoginBarCode" })]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        SND_DBContext _userContext = new SND_DBContext();
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();

        }
        public IActionResult Login()
        {
            return View();
        }
      
        public IActionResult LoginBarCode(string Username)
        {
            var barcode = Username;
            var sessionID = HttpContext?.Session.Get<User_UserBase>("UserLogin");

            if (sessionID != null)
            {
                return RedirectToAction("Index", "Home");
            }

            var UserCheck = _userContext.User_UserBases.Where(x => x.BarCode == barcode).FirstOrDefault();

            if (UserCheck!= null)
            {
                var recheckAccess = _userContext.User_Accesses.Where(x => x.IDUser == UserCheck.BarCode).FirstOrDefault();
                if (recheckAccess != null) 
                {
                    if(recheckAccess.Store.Contains("ManagePickUp") || recheckAccess.Store.Contains("Approve") || recheckAccess.Tool.Contains("Approve")|| recheckAccess.Tool.Contains("ManageBorrow")|| recheckAccess.UserManage.Contains("EditUser")|| recheckAccess.UserManage.Contains("SND_Organize"))
                    {
                        ViewData["ErrorLogin"] = "you have approve access please login by username";

                        return RedirectToAction("LogIn");

                    }
                    else
                    {
                        HttpContext.Session.Set<User_UserBase>("UserLogin", UserCheck);

                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ViewData["ErrorLogin"] = "Login fail";

            return RedirectToAction("LogIn");
        }

        public IActionResult LoginByUser(User_UserBase sndUser)
        {
            if (sndUser == null || string.IsNullOrEmpty(sndUser.Username) || string.IsNullOrEmpty(sndUser.Password))
            {
                TempData["ErrorLogin"] = "Username or Password cannot be empty.";
                return RedirectToAction("Login");
            }

            var UserCheck = _userContext.User_UserBases.FirstOrDefault(x => x.Username == sndUser.Username);

            if (UserCheck == null || sndUser.Password != UserCheck.Password)
            {
                TempData["ErrorLogin"] = "Invalid username or password.";
                return RedirectToAction("Login");
            }

            HttpContext.Session.Set<User_UserBase>("UserLogin", UserCheck);
            return RedirectToAction("Index", "Home");
        }


        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
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
