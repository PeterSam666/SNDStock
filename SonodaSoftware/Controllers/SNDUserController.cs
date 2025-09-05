using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SonodaSoftware.Data;
using SonodaSoftware.Models;
using SonodaSoftware.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SonodaSoftware.Controllers
{
    [SessionFilter]
    public class SNDUserController : Controller
    {
        SND_DBContext sND_User = new SND_DBContext();
        // GET: SNDUserController
        public ActionResult AllUser()
        {
            var user = sND_User.User_UserBases.ToList();
            var iduser = HttpContext.Session.Get<User_UserBase>("UserLogin");
            var checkAccess = sND_User.User_Accesses.FirstOrDefault(x => x.IDUser == iduser.BarCode)?.UserManage;
            ViewData["checkAccess"] = checkAccess;
            ViewData["user"] = user;
            return View();
        }

        public JsonResult getAccess(string Barcode)
        {
            var access = sND_User.User_Accesses.Where(x => x.IDUser == Barcode).FirstOrDefault();
            if (access != null)
            {
                var store = access.Store.Split(",");
                var tool = access.Tool.Split(",");
                var user = access.UserManage.Split(",");
                return Json(new { store, tool, user });
            }
            return Json("");
        }

        public IActionResult EditUserBase(User_UserBase User)
        {
            if (User != null)
            {
                var checkUser = sND_User.User_UserBases.Where(x => x.BarCode == User.BarCode).FirstOrDefault();
                var date = DateTime.Now;
                User.ID = checkUser.ID;
                User.Createdon = checkUser.Createdon;
                User.Modify_On = date;
                User.Username = checkUser.Username;
                User.Password = checkUser.Password;
                var trackedEntity = sND_User.User_UserBases
                        .Local
                        .FirstOrDefault(e => e.ID == User.ID);

                if (trackedEntity != null)
                {
                    // ถ้ามี entity ที่ถูกติดตามอยู่ ให้ทำการอัปเดตค่า
                    sND_User.Entry(trackedEntity).CurrentValues.SetValues(User);
                }
                else
                {
                    // ถ้าไม่มี ให้ทำการ Update ปกติ
                    sND_User.User_UserBases.Update(User);
                }
                sND_User.SaveChanges();
            }
            return RedirectToAction("AllUser");
        }

        public JsonResult EditAccess(string[][] valueAccess, string barcode)
        {
            User_Access access = new User_Access();
            var recheckSave = false;
            var StockAccess = "";
            var ToolAccess = "";
            var UserAccess = "";
            if (barcode != null)
            {
                if (valueAccess.Length >= 2)
                {
                    try
                    {
                        foreach (var Store in valueAccess[0])
                        {
                            StockAccess += $"{Store.ToString()},";
                        }
                        foreach (var Tool in valueAccess[1])
                        {
                            ToolAccess += $"{Tool.ToString()},";
                        }
                        foreach (var User in valueAccess[2])
                        {
                            UserAccess += $"{User.ToString()},";
                        }
                        StockAccess = StockAccess.Remove(StockAccess.Length - 1);
                        ToolAccess = ToolAccess.Remove(ToolAccess.Length - 1);
                        UserAccess = UserAccess.Remove(UserAccess.Length - 1);
                        access.IDUser = barcode;
                        access.Store = StockAccess;
                        access.Tool = ToolAccess;
                        access.UserManage = UserAccess;
                        var trackedEntity = sND_User.User_Accesses
                            .Local
                            .FirstOrDefault(e => e.IDUser == access.IDUser);

                        if (trackedEntity != null)
                        {
                            // ถ้ามี entity ที่ถูกติดตามอยู่ ให้ทำการอัปเดตค่า
                            sND_User.Entry(trackedEntity).CurrentValues.SetValues(access);
                        }
                        else
                        {
                            // ถ้าไม่มี ให้ทำการ Update ปกติ
                            sND_User.User_Accesses.Update(access);
                        }
                        sND_User.SaveChanges();
                        recheckSave = true;
                    }
                    catch
                    {

                    }
                }
            }
            return Json(new { Success = recheckSave });
        }

        public IActionResult EditOrganize()
        {
            return View();
        }

        public JsonResult getOrganizePerson(int departmentType)
        {
            var typePerson = sND_User.User_Organizes.Where(x => x.DepartmentType == departmentType).FirstOrDefault();
            if (typePerson != null)
            {
                HttpContext.Session.Set<int>("departmentType", departmentType);
                return Json(typePerson);
            }
            return Json("");
        }

        public IActionResult saveOrganizeChart(User_Organize organize)
        {
            try
            {
                if (organize.VicePresident != null)
                {
                    organize.DepartmentType = HttpContext.Session.Get<int>("departmentType");
                    var check = sND_User.User_Organizes.FirstOrDefault();
                    if (check != null)
                    {
                        var trackedEntity = sND_User.User_Organizes
                            .Local
                            .FirstOrDefault(e => e.DepartmentType == organize.DepartmentType);

                        if (trackedEntity != null)
                        {
                            // ถ้ามี entity ที่ถูกติดตามอยู่ ให้ทำการอัปเดตค่า
                            sND_User.Entry(trackedEntity).CurrentValues.SetValues(organize);
                        }
                        else
                        {
                            // ถ้าไม่มี ให้ทำการ Update ปกติ
                            sND_User.User_Organizes.Update(organize);
                        }
                    }
                    else
                    {
                        sND_User.User_Organizes.Add(organize);
                    }
                    sND_User.SaveChanges();
                }

            }
            catch
            {

            }
            return RedirectToAction("EditOrganize");
        }

        public ActionResult ADDUser()
        {
            return View();
        }

        public JsonResult SaveAccess(string[][] valueAccess, string barcode)
        {
            User_Access access = new User_Access();
            var recheckSave = false;
            var StockAccess = "";
            var ToolAccess = "";
            var UserAccess = "";
            if (barcode != null)
            {
                if (valueAccess.Length >= 2)
                {
                    try
                    {
                        foreach (var Store in valueAccess[0])
                        {
                            StockAccess += $"{Store.ToString()},";
                        }
                        foreach (var Tool in valueAccess[1])
                        {
                            ToolAccess += $"{Tool.ToString()},";
                        }
                        foreach (var User in valueAccess[2])
                        {
                            UserAccess += $"{User.ToString()},";
                        }
                        StockAccess = StockAccess.Remove(StockAccess.Length - 1);
                        ToolAccess = ToolAccess.Remove(ToolAccess.Length - 1);
                        UserAccess = UserAccess.Remove(UserAccess.Length - 1);
                        access.IDUser = barcode;
                        access.Store = StockAccess;
                        access.Tool = ToolAccess;
                        access.UserManage = UserAccess;
                        sND_User.User_Accesses.Add(access);
                        sND_User.SaveChanges();
                        recheckSave = true;
                    }
                    catch
                    {

                    }
                }
            }
            return Json(new { Success = recheckSave });
        }

        public IActionResult SaveUserBase(User_UserBase User)
        {
            if (User != null)
            {
                var checkUser = sND_User.User_UserBases.Where(x => x.BarCode == User.BarCode).FirstOrDefault();
                if (checkUser == null)
                {
                    var date = DateTime.Now;
                    User.Createdon = date;
                    User.Username = $"{User.FirstName_ENG}_{User.LastName_ENG[0]}";
                    User.Password = $"{User.Nickname}@Sonoda";
                    sND_User.User_UserBases.Add(User);
                    sND_User.SaveChanges();
                }

            }
            return RedirectToAction("ADDUser");
        }
    }
}
