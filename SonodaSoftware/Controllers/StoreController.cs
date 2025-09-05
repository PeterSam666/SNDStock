using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using NuGet.Versioning;
using SonodaSoftware.Data;
using SonodaSoftware.Models;
using SonodaSoftware.Services;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SonodaSoftware.Controllers
{
    [SessionFilter]
    public class StoreController : Controller
    {
        SND_DBContext context = new SND_DBContext();
        public ActionResult Index()
        {
            return View();
            //return RedirectToAction("Checkstock", "Home");
        }
        public ActionResult PartSearch()
        {
            var itemJson = HttpContext.Session.GetString("part");
            var item = string.IsNullOrEmpty(itemJson) ? new List<Store_Part>() : JsonConvert.DeserializeObject<List<Store_Part>>(itemJson);
            ViewData["part"] = item;
            return View();
        }

        public ActionResult partsearchResult(string keyword, string SearchBy)
        {
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction("PartSearch");

            var part = new List<Store_Part>();

            switch (SearchBy)
            {
                case "ID":
                    part = context.Store_Parts
                        .Where(x => x.ID.ToString().Contains(keyword) && x.status == true)
                        .OrderBy(x => x.ID_Product)
                        .ToList();
                    break;
                case "Barcode_ID":
                    part = context.Store_Parts
                        .Where(x => x.ID_Product.ToString().Contains(keyword) && x.status == true)
                        .OrderBy(x => x.ID_Product)
                        .ToList();
                    break;
                case "NAME_PRODUCT_ENG":
                    part = context.Store_Parts
                        .Where(x => x.NAME_PRODUCT_ENG.ToString().Contains(keyword) && x.status == true)
                        .OrderBy(x => x.ID_Product)
                        .ToList();
                    break;
                case "NAME_PRODUCT_TH":
                    part = context.Store_Parts
                        .Where(x => x.NAME_PRODUCT_TH.ToString().Contains(keyword) && x.status == true)
                        .OrderBy(x => x.ID_Product)
                        .ToList();
                    break;
                case "Location":
                    part = context.Store_Parts
                        .Where(x => x.Location.ToString().Contains(keyword) && x.status == true)
                        .OrderBy(x => x.ID_Product)
                        .ToList();
                    break;
                default:
                    break;
            }
            HttpContext.Session.SetString("part", JsonConvert.SerializeObject(part));
            return RedirectToAction("PartSearch");
        }


        public ActionResult AddPart(Store_AddPart part)
        {
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            part.Maker = user.BarCode;
            part.TimeStamp = DateTime.Now;
            context.Store_AddParts.Add(part);
            context.SaveChanges();
            Store_Approve _Approve = new Store_Approve();
            _Approve.Apporve_Type = 0;
            _Approve.Apporve_Type_Name = "AddPart";
            _Approve.Date = DateTime.Now;
            _Approve.Request_ID = user.ID;
            _Approve.Request_Name = user.Username;
            _Approve.Apporve_status = 0;
            _Approve.Apporve_Text = "wait Approve";
            _Approve.Referent_EventID = context.Store_AddParts.Where(x => x.ID_Product == part.ID_Product).FirstOrDefault()?.ID;
            context.Store_Approves.Add(_Approve);
            context.SaveChanges();
            TempData["AddPart"] = "Success";
            return RedirectToAction("PartSearch");
        }

        public JsonResult Editpart(int ID)
        {
            var part = context.Store_Parts.Where(x => x.ID == ID).FirstOrDefault();
            return Json(part);
        }

        public JsonResult SaveEditpart(Store_Part EditPart)
        {
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            var part = context.Store_Parts.Where(x => x.ID == EditPart.ID).FirstOrDefault();
            Store_EditPart partEdit = new Store_EditPart();
            partEdit.Maker = user.BarCode;
            partEdit.TimeStamp = DateTime.Now;
            partEdit.ID_Product = EditPart.ID_Product;
            partEdit.Before_NameEng = part.NAME_PRODUCT_ENG;
            partEdit.Before_NameTH = part.NAME_PRODUCT_TH;
            partEdit.Before_Quantity = part.QUANTITY;
            partEdit.Before_Unit = part.UNIT;
            partEdit.Before_UNIT_PRICE = part.UNIT_PRICE;
            partEdit.Before_Location = EditPart.Location;
            partEdit.Before_status = part.status;
            partEdit.After_NameEng = EditPart.NAME_PRODUCT_ENG;
            partEdit.After_NameTH = EditPart.NAME_PRODUCT_TH;
            partEdit.After_Quantity = EditPart.QUANTITY;
            partEdit.After_Unit = EditPart.UNIT;
            partEdit.After_UNIT_PRICE = EditPart.UNIT_PRICE;
            partEdit.After_Location = EditPart.Location;
            partEdit.After_status = true;
            context.Store_EditParts.Add(partEdit);
            context.SaveChanges();
            Store_Approve _Approve = new Store_Approve();
            _Approve.Apporve_Type = 1;
            _Approve.Apporve_Type_Name = "EditPart";
            _Approve.Date = DateTime.Now;
            _Approve.Request_ID = user.ID;
            _Approve.Request_Name = user.Username;
            _Approve.Apporve_status = 0;
            _Approve.Apporve_Text = "wait Approve";
            _Approve.Referent_EventID = partEdit.ID;
            context.Store_Approves.Add(_Approve);
            context.SaveChanges();
            TempData["AddPart"] = "Success";
            return Json("Success");
        }


        public ActionResult Report()
        {
            var HistoryPickUp = TempData["date"] as List<Store_PickUp>;
            if (HistoryPickUp == null)
            {
                var dateSearch = DateTime.Now;
                HistoryPickUp = context.Store_PickUps.Where(x => x.Date.Day == dateSearch.Day && x.Date.Month == dateSearch.Month && x.Date.Year == dateSearch.Year)
                    .OrderBy(x => x.Date).ThenBy(x => x.User_ID).ToList();
            }

            ViewData["pickup_his"] = HistoryPickUp;
            return View();
        }
        public ActionResult SearchReport(int reportType, string dateStart, string dateEnd)
        {
            if (!(dateStart == "" || dateEnd == ""))
            {

                var start = convertdate(dateStart);
                var end = convertdate(dateEnd);
                var HistoryPickUp = context.Store_PickUps.Where(x => x.Date >= start && x.Date <= end)
                    .OrderBy(x => x.Date).ThenBy(x => x.User_ID).ToList();
                TempData["date"] = HistoryPickUp;
            }
            return RedirectToAction("History");
        }
        private DateTime convertdate(string date)
        {
            var converseFormat = date.Split('/');
            var dateResult = new DateTime(Convert.ToInt32(converseFormat[2]), Convert.ToInt32(converseFormat[1]), Convert.ToInt32(converseFormat[0]));
            return dateResult;
        }

        public JsonResult ViewDetail(string ID)
        {
            var IDPickUp = Convert.ToInt32(ID);
            var detail = context.Store_PickUp_Details.Where(x => x.PickUpID == IDPickUp).ToList();
            return Json(detail);
        }

        public ActionResult PickUp()
        {
            ViewData["addPickUp"] = HttpContext?.Session.Get<string>("addPickUp");
            ViewData["message"] = HttpContext?.Session.Get<string>("message");
            ViewData["part"] = HttpContext?.Session.Get<List<PartModel>>("part");
            var group = context.Store_PartGroups.OrderBy(x => x.NAME_GROUP_EN).ToList();
            ViewData["group"] = group;
            ViewData["ListPick"] = HttpContext?.Session.Get<List<PartModel>>("ListPick");
            return View();
        }

        public ActionResult BarCodePickUp(string BarCode)
        {
            var part = context.Store_Parts.Where(x => x.ID_Product == BarCode && x.status == true).Select(x => new PartModel
            {
                ID = x.ID,
                ID_PRODUCT = x.ID_Product,
                NAME_PRODUCT_ENG = x.NAME_PRODUCT_ENG,
                NAME_PRODUCT_TH = x.NAME_PRODUCT_TH,
                QUANTITY = x.QUANTITY
            }).OrderBy(x => x.ID_PRODUCT).ToList();

            // Save `part` to Session
            HttpContext.Session.Set<List<PartModel>>("part", part);
            return RedirectToAction("PickUp");
        }

        public ActionResult pickUpResult(string group)
        {
            var ID_group = context.Store_PartGroups.Where(x => x.NAME_GROUP_EN == group).FirstOrDefault()?.ID_TYPE;
            var part = context.Store_Parts.Where(x => x.ID_Product.Contains(ID_group) && x.status == true).Select(x => new PartModel
            {
                ID = x.ID,
                ID_PRODUCT = x.ID_Product,
                NAME_PRODUCT_ENG = x.NAME_PRODUCT_ENG,
                NAME_PRODUCT_TH = x.NAME_PRODUCT_TH,
                QUANTITY = x.QUANTITY
            }).OrderBy(x => x.ID_PRODUCT).ToList();

            // Save `part` to Session
            HttpContext.Session.Set<List<PartModel>>("part", part);
            return RedirectToAction("PickUp");
        }

        public ActionResult addPickUplist(PartModel part)
        {
            var PickUps = HttpContext?.Session.Get<List<PartModel>>("ListPick");
            var check = checkNum(part.ID, part.QUANTITY);

            if (check)
            {
                if (PickUps == null) PickUps = new List<PartModel>();
                var reCheckName = PickUps.Where(x => x.ID == part.ID).FirstOrDefault();
                if (reCheckName != null)
                {
                    if (checkNum(reCheckName.ID, reCheckName.QUANTITY + part.QUANTITY))
                    {
                        PickUps.Add(part);
                        var listSum = PickUps.GroupBy(x => x.ID, (key, g) => new PartModel
                        {
                            ID = key,
                            ID_PRODUCT = g.FirstOrDefault()?.ID_PRODUCT,
                            NAME_PRODUCT_ENG = g.FirstOrDefault()?.NAME_PRODUCT_ENG,
                            NAME_PRODUCT_TH = g.FirstOrDefault().NAME_PRODUCT_TH,
                            QUANTITY = g.Sum(x => x.QUANTITY)
                        }).OrderBy(x => x.ID_PRODUCT).ToList();
                        PickUps.Clear();
                        PickUps.AddRange(listSum);
                    }
                    else
                    {
                        HttpContext.Session.Set<string>("addPickUp", "the quantity of stock is less than the pickup amount. Please check and input the quantity again");
                        HttpContext.Session.Set<List<PartModel>>("ListPick", PickUps);
                        return RedirectToAction("PickUp");
                    }
                }
                else
                {
                    PickUps.Add(part);
                }
                HttpContext.Session.Set<string>("addPickUp", "");
            }
            else
            {
                HttpContext.Session.Set<string>("addPickUp", "the quantity of stock is less than the pickup amount. Please check and input the quantity again");
            }

            HttpContext.Session.Set<List<PartModel>>("ListPick", PickUps);
            return RedirectToAction("PickUp");
        }


        private bool checkNum(int id, int? QUANTITY)
        {
            var store = context.Store_Parts.Where(x => x.ID == id && x.status == true).FirstOrDefault()?.QUANTITY ?? 0;
            if (QUANTITY <= store) return true;
            return false;
        }

        public JsonResult Remove(string id)
        {
            var Id = new int();
            if (id != "No data available in table")
            {
                Id = Convert.ToInt32(id);
                var list = HttpContext?.Session.Get<List<PartModel>>("ListPick");
                var dataRemove = list.Where(x => x.ID == Id).FirstOrDefault();
                list.Remove(dataRemove);
                HttpContext.Session.Set<List<PartModel>>("ListPick", list);
            }
            return Json(new { data = Id });
        }

        public ActionResult PickUpSave(string job, string Machine, string PickupFor)
        {
            var pickUp = HttpContext?.Session.Get<List<PartModel>>("ListPick");
            //var pickUp = Session["PickUplist"] as List<PartModel>;
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            try
            {
                Store_PickUp sND_Pick_Up_ = new Store_PickUp();
                sND_Pick_Up_.Date = DateTime.Now;
                sND_Pick_Up_.Job = job;
                sND_Pick_Up_.Machine_Code = Machine;
                if (PickupFor != null)
                {
                    sND_Pick_Up_.PickUpBy = user.BarCode;
                    var findPIckUpFor = context.User_UserBases.Where(x => x.BarCode.Contains(PickupFor)).ToList();
                    var PIckUpFor = findPIckUpFor.Where(x => x.BarCode.Split('-')[0].Contains(PickupFor)).FirstOrDefault();
                    sND_Pick_Up_.User_ID = PIckUpFor.BarCode;
                    sND_Pick_Up_.Reveal_Name = $"{PIckUpFor.FirstName_ENG}_{PIckUpFor.LastName_ENG[0]}";
                }
                else
                {
                    sND_Pick_Up_.PickUpBy = user.BarCode;
                    sND_Pick_Up_.User_ID = user.BarCode;
                    sND_Pick_Up_.Reveal_Name = $"{user.FirstName_ENG}_{user.LastName_ENG[0]}";
                }
                sND_Pick_Up_.Total = pickUp.Sum(x => x.QUANTITY);
                sND_Pick_Up_.Description = "";
                sND_Pick_Up_.Remark = "";

                context.Store_PickUps.Add(sND_Pick_Up_);
                context.SaveChanges();
                Store_Approve sND_Approve = new Store_Approve();
                sND_Approve.Apporve_Type = 3;
                sND_Approve.Apporve_Type_Name = "Pick Up";
                sND_Approve.Date = DateTime.Now;
                if (PickupFor != null)
                {
                    var PIckUpFor = context.User_UserBases.Where(x => x.BarCode.Contains(PickupFor)).FirstOrDefault();
                    sND_Approve.Request_ID = PIckUpFor.ID;
                    sND_Approve.Request_Name = $"{PIckUpFor.FirstName_ENG} _ {PIckUpFor.LastName_ENG[0]}";
                }
                else
                {
                    sND_Approve.Request_ID = user.ID;
                    sND_Approve.Request_Name = $"{user.FirstName_ENG} _ {user.LastName_ENG[0]}";
                }
                sND_Approve.Apporve_status = 0;
                sND_Approve.Apporve_Text = "Wait Approve";
                sND_Approve.Referent_EventID = sND_Pick_Up_.ID;
                context.Store_Approves.Add(sND_Approve);
                context.SaveChanges();
                foreach (var part in pickUp)
                {
                    var partDetail = context.Store_Parts.Where(x => x.ID == part.ID && x.status == true).FirstOrDefault();
                    Store_PickUp_Detail pick_Up_Detail = new Store_PickUp_Detail();
                    pick_Up_Detail.ID_Product = partDetail.ID_Product;
                    pick_Up_Detail.Name_TH = partDetail.NAME_PRODUCT_TH;
                    pick_Up_Detail.Name_EN = partDetail.NAME_PRODUCT_ENG;
                    pick_Up_Detail.Item_QUANTITY = part.QUANTITY;
                    pick_Up_Detail.PickUpID = sND_Pick_Up_.ID;
                    context.Store_PickUp_Details.Add(pick_Up_Detail);
                    context.SaveChanges();
                }
                pickUp.Clear();
                var accessUser = context.User_Accesses.Where(x => x.IDUser == user.BarCode).FirstOrDefault().Store;
                if (accessUser.Contains("Approve"))
                {
                    var PickUp = context.Store_PickUps.Where(x => x.ID == sND_Pick_Up_.ID).FirstOrDefault();
                    PickUp.ApproveTime = DateTime.Now;
                    PickUp.ApproveBy = user.BarCode;
                    PickUp.ApproveID = sND_Approve.ID;
                    context.Store_PickUps.Update(PickUp);
                    context.SaveChanges();
                    var pickupDetail = context.Store_PickUp_Details.Where(x => x.PickUpID == PickUp.ID).ToList();
                    foreach (var item in pickupDetail)
                    {
                        var partPickup = context.Store_Parts.Where(x => x.ID_Product == item.ID_Product).FirstOrDefault();
                        partPickup.QUANTITY -= item.Item_QUANTITY;
                        context.Store_Parts.Update(partPickup);
                    }
                    context.SaveChanges();
                    var updatePart = HttpContext?.Session.Get<List<PartModel>>("part");
                    var Update = updatePart.Select(x => x.ID_PRODUCT).FirstOrDefault().Split("-");
                    var partforUpdate = context.Store_Parts.Where(x => x.ID_Product.Contains(Update[0])).Select(x => new PartModel
                    {
                        ID = x.ID,
                        ID_PRODUCT = x.ID_Product,
                        NAME_PRODUCT_ENG = x.NAME_PRODUCT_ENG,
                        NAME_PRODUCT_TH = x.NAME_PRODUCT_TH,
                        QUANTITY = x.QUANTITY
                    }).ToList();
                    HttpContext?.Session.Set<List<PartModel>>("part", partforUpdate);
                    var approveUpdate = context.Store_Approves.Where(x => x.ID == sND_Approve.ID).FirstOrDefault();
                    approveUpdate.Apporve_status = 1;
                    approveUpdate.Apporve_Text = "Approved";
                    approveUpdate.Apporve_date = DateTime.Now;
                    context.Store_Approves.Update(approveUpdate);
                    context.SaveChanges();
                }

            }
            catch (Exception)
            {
                TempData["message"] = "Fail";
                HttpContext.Session.Set<List<PartModel>>("ListPick", pickUp);
                //Session["PickUplist"] = pickUp;
                return RedirectToAction("PickUp");
            }
            TempData["message"] = "Success";
            HttpContext.Session.Set<List<PartModel>>("ListPick", pickUp);
            //Session["PickUplist"] = pickUp;
            return RedirectToAction("PickUp");
        }

        public IActionResult GetPickupHistory()
        {
            var fiveDaysAgo = DateTime.Now.AddDays(-5);

            var main = context.Store_PickUps
                .Where(p => p.Date >= fiveDaysAgo)
                .OrderByDescending(p => p.Date)
                .Select(p => new
                {
                    p.ID,
                    p.Date,
                    p.Job,
                    MachineCode = p.Machine_Code,
                    p.PickUpBy,
                    p.Total
                })
            .ToList();

            var detail = context.Store_PickUp_Details
                .Where(d => d.PickUp.Date >= fiveDaysAgo)
                .OrderByDescending(d => d.PickUpID)
                .Select(d => new
                {
                    d.ID,
                    d.PickUpID,
                    ProductID = d.ID_Product,
                    NameTH = d.Name_TH,
                    Quantity = d.Item_QUANTITY
                })
                .ToList();

            return Ok(new { main, detail });
        }

        // ✅ ยกเลิก Pickup Detail + อัปเดตข้อมูล
        public IActionResult CancelPickup([FromBody] CancelPickupRequest request)
        {
            using var transaction = context.Database.BeginTransaction();

            try
            {
                // แปลงเฉพาะ Id, PickUpId, Quantity
                if (!int.TryParse(request.Id, out int detailId) ||
                    !int.TryParse(request.PickUpId, out int pickupId) ||
                    !int.TryParse(request.Quantity, out int quantity))
                {
                    return BadRequest("ข้อมูลไม่ถูกต้อง (ไม่สามารถแปลงเป็นตัวเลขได้)");
                }

                // ดึงข้อมูล detail
                var detail = context.Store_PickUp_Details.FirstOrDefault(d => d.ID == detailId);
                if (detail == null)
                    return NotFound("ไม่พบรายการที่ต้องการยกเลิก");

                // ลบ detail
                context.Store_PickUp_Details.Remove(detail);

                // อัปเดต Pickup
                var pickup = context.Store_PickUps.FirstOrDefault(p => p.ID == pickupId);
                if (pickup != null)
                {
                    pickup.Total = Math.Max(0, pickup.Total - quantity);

                    if (pickup.Total == 0)
                    {
                        context.Store_PickUps.Remove(pickup); // 🔴 ลบถ้า Total เหลือ 0
                    }
                    else
                    {
                        context.Store_PickUps.Update(pickup);
                    }
                }

                // คืนสินค้าเข้า stock โดย ProductId ยังเป็น string
                var part = context.Store_Parts.FirstOrDefault(p => p.ID_Product.ToString() == request.ProductId);
                if (part != null)
                {
                    part.QUANTITY += quantity;
                }

                context.SaveChanges();
                transaction.Commit();

                return Ok(new { message = "ยกเลิกสำเร็จ" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }



        // ✅ โมเดลสำหรับรับข้อมูลยกเลิก
        public class CancelPickupRequest
        {
            public string Id { get; set; }
            public string PickUpId { get; set; }
            public string ProductId { get; set; }
            public string Quantity { get; set; }
        }

        public ActionResult Receive()
        {
            return View();
        }

        public ActionResult Approve()
        {
            var apprv = context.Store_Approves.Where(x => x.Apporve_status == 0).ToList();
            ViewData["approve"] = apprv;
            return View();
        }

        public JsonResult DetailApprove(int id, int approveType)
        {
            var detail = "";
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            var approve = context.Store_Approves.Where(x => x.ID == id).FirstOrDefault();
            switch (approveType)
            {
                case 0:
                    var addpart = context.Store_AddParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                    detail = JsonConvert.SerializeObject(addpart);
                    break;
                case 1:
                    var Editpart = context.Store_EditParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                    detail = JsonConvert.SerializeObject(Editpart);
                    break;
                case 2:
                    var DeletePart = context.Store_DeleteParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                    detail = JsonConvert.SerializeObject(DeletePart);
                    break;
                case 3:
                    var pickUp = context.Store_PickUps.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                    var pickupDetail = context.Store_PickUp_Details.Where(x => x.PickUpID == pickUp.ID).ToList();
                    var pickUplist = new { pickUp, pickupDetail };

                    detail = JsonConvert.SerializeObject(pickUplist, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore // ข้ามการอ้างอิงซ้ำ
                    });
                    break;
                case 4:
                    var Do = context.Store_Dos.FirstOrDefault(x => x.ID == approve.Referent_EventID);
                    var Detail = context.Store_Do_Datails.Where(x => x.DO_ID == Do.ID).ToList();
                    var Dolist = new { Do, Detail };
                    detail = JsonConvert.SerializeObject(Dolist, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore // ข้ามการอ้างอิงซ้ำ
                    });
                    break;
                case 5:
                    var AddSupplier = context.Store_AddSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                    detail = JsonConvert.SerializeObject(AddSupplier);
                    break;
                case 6:
                    var EditSupplier = context.Store_EditSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                    detail = JsonConvert.SerializeObject(EditSupplier);
                    break;
                case 7:
                    var DeledteSupplier = context.Store_DeleteSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                    detail = JsonConvert.SerializeObject(DeledteSupplier);
                    break;
                default:
                    break;
            }
            return Json(detail);
        }

        public JsonResult ApproveSuccess(int Id, int approveType, string Reason)
        {
            var status = "fail";
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            var approve = context.Store_Approves.Where(x => x.ID == Id).FirstOrDefault();
            try
            {
                switch (approveType)
                {
                    case 0:
                        var addpart = context.Store_AddParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        addpart.ApproveTime = DateTime.Now;
                        addpart.ApproveBy = user.BarCode;
                        addpart.ApproveID = Id;
                        context.Store_AddParts.Update(addpart);
                        context.SaveChanges();
                        Store_Part part = new Store_Part();
                        part.ID_Product = addpart.ID_Product;
                        part.NAME_PRODUCT_ENG = addpart.NAME_PRODUCT_ENG;
                        part.NAME_PRODUCT_TH = addpart.NAME_PRODUCT_TH;
                        part.MAKER = user.Username;
                        part.QUANTITY = addpart.QUANTITY;
                        part.UNIT = addpart.UNIT;
                        part.UNIT_PRICE = addpart.UNIT_PRICE;
                        part.Description = addpart.Description;
                        part.Location = addpart.Location;
                        part.Created_On = addpart.TimeStamp;
                        part.status = true;
                        context.Store_Parts.Add(part);
                        context.SaveChanges();
                        break;
                    case 1:
                        var Editpart = context.Store_EditParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        Editpart.ApproveTime = DateTime.Now;
                        Editpart.ApproveBy = user.BarCode;
                        Editpart.ApproveID = Id;
                        context.Store_EditParts.Update(Editpart);
                        context.SaveChanges();
                        var partEdit = context.Store_Parts.Where(x => x.ID_Product == Editpart.ID_Product).FirstOrDefault();
                        partEdit.ID_Product = Editpart.ID_Product;
                        partEdit.NAME_PRODUCT_ENG = Editpart.After_NameEng ?? partEdit.NAME_PRODUCT_ENG;
                        partEdit.NAME_PRODUCT_TH = Editpart.After_NameTH ?? partEdit.NAME_PRODUCT_TH;
                        partEdit.QUANTITY = Editpart.After_Quantity ?? partEdit.QUANTITY;
                        partEdit.UNIT = Editpart.After_Unit ?? partEdit.UNIT;
                        partEdit.UNIT_PRICE = Editpart.After_UNIT_PRICE ?? partEdit.UNIT_PRICE;
                        partEdit.Location = Editpart.After_Location ?? partEdit.Location;
                        partEdit.Modify_On = Editpart.TimeStamp;
                        context.Store_Parts.Update(partEdit);
                        context.SaveChanges();
                        break;
                    case 2:
                        var DeletePart = context.Store_DeleteParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        DeletePart.ApproveTime = DateTime.Now;
                        DeletePart.ApproveBy = user.BarCode;
                        DeletePart.ApproveID = Id;
                        context.Store_DeleteParts.Update(DeletePart);
                        context.SaveChanges();
                        var partDelete = context.Store_Parts.Where(x => x.ID_Product == DeletePart.ID_Product).FirstOrDefault();
                        partDelete.status = false;
                        context.Store_Parts.Update(partDelete);
                        context.SaveChanges();
                        break;
                    case 3:
                        var PickUp = context.Store_PickUps.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        PickUp.ApproveTime = DateTime.Now;
                        PickUp.ApproveBy = user.BarCode;
                        PickUp.ApproveID = Id;
                        context.Store_PickUps.Update(PickUp);
                        context.SaveChanges();
                        var pickupDetail = context.Store_PickUp_Details.Where(x => x.PickUpID == PickUp.ID).ToList();
                        foreach (var item in pickupDetail)
                        {
                            var partPickup = context.Store_Parts.Where(x => x.ID_Product == item.ID_Product).FirstOrDefault();
                            partPickup.QUANTITY -= item.Item_QUANTITY;
                            context.Store_Parts.Update(partPickup);
                        }
                        context.SaveChanges();
                        break;
                    case 4:
                        var Do = context.Store_Dos.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        Do.ApproveTime = DateTime.Now;
                        Do.ApproveBy = user.BarCode;
                        Do.ApproveID = Id;
                        context.Store_Dos.Update(Do);
                        context.SaveChanges();
                        var Detail = context.Store_Do_Datails.Where(x => x.DO_ID == Do.ID).ToList();
                        foreach (var item in Detail)
                        {
                            var partDo = context.Store_Parts.Where(x => x.ID_Product == item.ID_Product).FirstOrDefault();
                            partDo.QUANTITY += item.Quantity ?? 0;
                            partDo.UNIT = item.Unit ?? partDo.UNIT;
                            partDo.UNIT_PRICE = item.Unit_Price ?? partDo.UNIT_PRICE;
                            context.Store_Parts.Update(partDo);
                        }
                        context.SaveChanges();
                        break;
                    case 5:
                        var AddSupplier = context.Store_AddSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        AddSupplier.ApproveTime = DateTime.Now;
                        AddSupplier.ApproveBy = user.BarCode;
                        AddSupplier.ApproveID = Id;
                        context.Store_AddSuppliers.Update(AddSupplier);
                        context.SaveChanges();
                        Store_Supplier Supplier = new Store_Supplier();
                        Supplier.SupplierID = AddSupplier.SupplierID;
                        Supplier.SupplierName = AddSupplier.SupplierName;
                        Supplier.AddressDetail = AddSupplier.AddressDetail;
                        Supplier.Country = AddSupplier.Country;
                        Supplier.Province = AddSupplier.Province;
                        Supplier.District = AddSupplier.District;
                        Supplier.Sub_District = AddSupplier.Sub_District;
                        Supplier.PostCode = AddSupplier.PostCode;
                        Supplier.Email = AddSupplier.Email;
                        Supplier.Email2 = AddSupplier.Email2;
                        Supplier.FirstName1 = AddSupplier.FirstName1;
                        Supplier.LastName1 = AddSupplier.LastName1;
                        Supplier.MobileNumber1 = AddSupplier.MobileNumber1;
                        Supplier.FirstName2 = AddSupplier.FirstName2;
                        Supplier.LastName2 = AddSupplier.LastName2;
                        Supplier.MobileNumber2 = AddSupplier.MobileNumber2;
                        Supplier.PhoneNumber1 = AddSupplier.PhoneNumber1;
                        Supplier.PhoneNumber2 = AddSupplier.PhoneNumber2;
                        Supplier.Fax1 = AddSupplier.Fax1;
                        Supplier.Fax2 = AddSupplier.Fax2;
                        Supplier.Credit = AddSupplier.Credit;
                        Supplier.Account_No = AddSupplier.Account_No;
                        Supplier.Created_On = AddSupplier.TimeStamp;
                        Supplier.status = true;
                        context.Store_Suppliers.Add(Supplier);
                        context.SaveChanges();
                        break;
                    case 6:
                        var EditSupplier = context.Store_EditSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        EditSupplier.ApproveTime = DateTime.Now;
                        EditSupplier.ApproveBy = user.BarCode;
                        EditSupplier.ApproveID = Id;
                        context.Store_EditSuppliers.Update(EditSupplier);
                        context.SaveChanges();
                        var newdata = JsonConvert.DeserializeObject<Store_Supplier>(EditSupplier.NewData);
                        var SupplierEdit = context.Store_Suppliers.Where(x => x.ID == EditSupplier.SupplierID).FirstOrDefault();
                        SupplierEdit.SupplierID = newdata.SupplierID ?? SupplierEdit.SupplierID;
                        SupplierEdit.SupplierName = newdata.SupplierName ?? SupplierEdit.SupplierName;
                        SupplierEdit.AddressDetail = newdata.AddressDetail ?? SupplierEdit.AddressDetail;
                        SupplierEdit.Country = newdata.Country ?? SupplierEdit.Country;
                        SupplierEdit.Province = newdata.Province ?? SupplierEdit.Province;
                        SupplierEdit.District = newdata.District ?? SupplierEdit.District;
                        SupplierEdit.Sub_District = newdata.Sub_District ?? SupplierEdit.Sub_District;
                        SupplierEdit.PostCode = newdata.PostCode ?? SupplierEdit.PostCode;
                        SupplierEdit.Email = newdata.Email ?? SupplierEdit.Email;
                        SupplierEdit.Email2 = newdata.Email2 ?? SupplierEdit.Email2;
                        SupplierEdit.FirstName1 = newdata.FirstName1 ?? SupplierEdit.FirstName1;
                        SupplierEdit.LastName1 = newdata.LastName1 ?? SupplierEdit.LastName1;
                        SupplierEdit.MobileNumber1 = newdata.MobileNumber1 ?? SupplierEdit.MobileNumber1;
                        SupplierEdit.FirstName2 = newdata.FirstName2 ?? SupplierEdit.FirstName2;
                        SupplierEdit.LastName2 = newdata.LastName2 ?? SupplierEdit.LastName2;
                        SupplierEdit.MobileNumber2 = newdata.MobileNumber2 ?? SupplierEdit.MobileNumber2;
                        SupplierEdit.PhoneNumber1 = newdata.PhoneNumber1 ?? SupplierEdit.PhoneNumber1;
                        SupplierEdit.PhoneNumber2 = newdata.PhoneNumber2 ?? SupplierEdit.PhoneNumber2;
                        SupplierEdit.Fax1 = newdata.Fax1 ?? SupplierEdit.Fax1;
                        SupplierEdit.Fax2 = newdata.Fax2 ?? SupplierEdit.Fax2;
                        SupplierEdit.Credit = newdata.Credit ?? SupplierEdit.Credit;
                        SupplierEdit.Account_No = newdata.Account_No ?? SupplierEdit.Account_No;
                        SupplierEdit.Modify_On = DateTime.Now;
                        SupplierEdit.status = newdata.status ?? SupplierEdit.status;
                        context.Store_Suppliers.Update(SupplierEdit);
                        context.SaveChanges();
                        break;
                    case 7:
                        var DeledteSupplier = context.Store_DeleteSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        DeledteSupplier.ApproveTime = DateTime.Now;
                        DeledteSupplier.ApproveBy = user.BarCode;
                        DeledteSupplier.ApproveID = Id;
                        context.Store_DeleteSuppliers.Update(DeledteSupplier);
                        context.SaveChanges();
                        var SupplierDelete = context.Store_Suppliers.Where(x => x.SupplierID == DeledteSupplier.SupplierID).FirstOrDefault();
                        SupplierDelete.status = false;
                        context.Store_Suppliers.Update(SupplierDelete);
                        context.SaveChanges();
                        break;
                    case 8:
                        var AddQTY = context.Store_AddQTies.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        AddQTY.ApproveTime = DateTime.Now;
                        AddQTY.ApproveBy = user.BarCode;
                        AddQTY.ApproveID = Id;
                        context.Store_AddQTies.Update(AddQTY);
                        context.SaveChanges();
                        var AddQTYPart = context.Store_Parts.Where(x => x.ID_Product == AddQTY.ID_Product).FirstOrDefault();
                        AddQTYPart.QUANTITY += AddQTY.Quantity ?? 0;
                        context.Store_Parts.Update(AddQTYPart);
                        break;

                    default:
                        break;
                }
                approve.Apporve_By_ID = user.ID;
                approve.Apporve_By_Name = user.Username;
                approve.Apporve_status = 1;
                approve.Apporve_Text = "Approved";
                approve.Apporve_date = DateTime.Now;
                approve.Reason = Reason;
                status = "success";
                context.Store_Approves.Update(approve);
                context.SaveChanges();
            }
            catch
            {
                return Json("fail");
            }
            return Json(status);
        }

        public JsonResult Reject(int Id, int approveType, string Reason)
        {
            var status = "fail";
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            var approve = context.Store_Approves.Where(x => x.ID == Id).FirstOrDefault();
            try
            {
                switch (approveType)
                {
                    case 0:
                        var addpart = context.Store_AddParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        addpart.ApproveBy = "Reject";
                        context.Store_AddParts.Update(addpart);
                        context.SaveChanges();
                        break;
                    case 1:
                        var Editpart = context.Store_EditParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        Editpart.ApproveBy = "Reject";
                        context.Store_EditParts.Update(Editpart);
                        context.SaveChanges();
                        break;
                    case 2:
                        var DeletePart = context.Store_DeleteParts.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        DeletePart.ApproveBy = "Reject";
                        context.Store_DeleteParts.Update(DeletePart);
                        context.SaveChanges();
                        break;
                    case 3:
                        var PickUp = context.Store_PickUps.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        PickUp.ApproveBy = "Reject";
                        context.Store_PickUps.Update(PickUp);
                        context.SaveChanges();
                        break;
                    case 4:
                        var Do = context.Store_Dos.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        Do.ApproveBy = "Reject";
                        context.Store_Dos.Update(Do);
                        context.SaveChanges();
                        break;
                    case 5:
                        var AddSupplier = context.Store_AddSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        AddSupplier.ApproveBy = "Reject";
                        context.Store_AddSuppliers.Update(AddSupplier);
                        context.SaveChanges();
                        break;
                    case 6:
                        var EditSupplier = context.Store_EditSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        EditSupplier.ApproveBy = "Reject";
                        context.Store_EditSuppliers.Update(EditSupplier);
                        context.SaveChanges();
                        break;
                    case 7:
                        var DeledteSupplier = context.Store_DeleteSuppliers.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        DeledteSupplier.ApproveBy = "Reject";
                        context.Store_DeleteSuppliers.Update(DeledteSupplier);
                        context.SaveChanges();
                        break;
                    case 8:
                        var AddQTY = context.Store_AddQTies.Where(x => x.ID == approve.Referent_EventID).FirstOrDefault();
                        AddQTY.ApproveBy = "Reject";
                        context.Store_AddQTies.Update(AddQTY);
                        context.SaveChanges();
                        break;
                    default:
                        break;
                }
                approve.Apporve_By_ID = user.ID;
                approve.Apporve_By_Name = user.Username;
                approve.Apporve_status = 2;
                approve.Apporve_Text = "Rejected";
                approve.Apporve_date = DateTime.Now;
                approve.Reason = Reason;
                context.Store_Approves.Update(approve);
                context.SaveChanges();
                status = "success";
            }
            catch
            {
            }
            return Json(status);
        }

        public ActionResult ReportPage()
        {
            return View();
        }

        public JsonResult getReport(int reportType, string startDAte, string EndDate)
        {
            var data = "fail";
            if (!(startDAte == null || EndDate == null))
            {
                var start = DateTime.Parse(startDAte);
                var end = DateTime.Parse(EndDate);
                if (reportType == 0)
                {
                    var column = new string[] { "TimeStamp", "ID_Product", "PartGroupType", "NAME_PRODUCT_ENG", "NAME_PRODUCT_TH", "AddBy", "SupplierID", "QUANTITY", "UNIT", "UNIT_PRICE", "Description", "Location", "ApproveBy", "ApproveTime" };
                    var rows = context.Store_AddParts.Where(x => x.TimeStamp.Date >= start.Date && x.TimeStamp.Date <= end.Date && x.ApproveTime != null).Select(x => new
                    {
                        TimeStamp = x.TimeStamp,
                        ID_Product = x.ID_Product,
                        PartGroupType = x.PartGroupType,
                        NAME_PRODUCT_ENG = x.NAME_PRODUCT_ENG,
                        NAME_PRODUCT_TH = x.NAME_PRODUCT_TH,
                        AddBy = x.Maker,
                        SupplierID = x.SupplierID,
                        QUANTITY = x.QUANTITY,
                        UNIT = x.UNIT,
                        UNIT_PRICE = x.UNIT_PRICE,
                        Description = x.Description,
                        Location = x.Location,
                        ApproveBy = x.ApproveBy,
                        ApproveTime = x.ApproveTime
                    }).ToList();
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);
                }
                else if (reportType == 1)
                {
                    var column = new string[] { "TimeStamp", "ID_Product","NAME_PRODUCT_ENG", "NAME_PRODUCT_TH", "AddBy", "SupplierID", "QUANTITY", "UNIT", "UNIT_PRICE", "Location", "ApproveBy", "ApproveTime" };
                    var rows = context.Store_EditParts.Where(x => x.TimeStamp.Date >= start.Date && x.TimeStamp.Date <= end.Date && x.ApproveTime != null).Select(x => new
                    {
                        TimeStamp = x.TimeStamp,
                        ID_Product = x.ID_Product,
                        NAME_PRODUCT_ENG = x.After_NameEng,
                        NAME_PRODUCT_TH = x.After_NameTH,
                        AddBy = x.Maker,
                        SupplierID = x.After_SupplierID,
                        QUANTITY = x.After_Quantity,
                        UNIT = x.After_Unit,
                        UNIT_PRICE = x.After_UNIT_PRICE,
                        Location = x.After_Location,
                        ApproveBy = x.ApproveBy,
                        ApproveTime = x.ApproveTime
                    }).ToList();
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);
                }
                else if (reportType == 2)
                {
                    var column = new string[] { "TimeStamp", "ID_Product","Description", "Location", "ApproveBy", "ApproveTime" };
                    var rows = context.Store_DeleteParts.Where(x => x.TimeStamp.Date >= start.Date && x.TimeStamp.Date <= end.Date && x.ApproveTime != null).Select(x => new
                    {
                        TimeStamp = x.TimeStamp,
                        ID_Product = x.ID_Product,
                        Description = x.Description,
                        ApproveBy = x.ApproveBy,
                        ApproveTime = x.ApproveTime
                    }).ToList();
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);
                }
                else if (reportType == 3)
                {
                    var column = new string[] { "Date", "Job", "Machine_Code", "Reveal_Name", "ID_Product", "Name_EN", "Item_QUANTITY", "PickUpID", "Total", "ApproveBy", "ApproveTime" };
                    var pickup = context.Store_PickUps.Where(x => x.Date.Date >= start.Date && x.Date.Date <= end && x.ApproveTime != null).ToList();
                    var rows = new List<reportPickUp>();
                    foreach (var item in pickup)
                    {
                        var detail = context.Store_PickUp_Details.Where(x => x.PickUpID == item.ID).Select(x => new reportPickUp
                        {
                            Date = item.Date,
                            ApproveBy = item.ApproveBy,
                            ID_Product = x.ID_Product,
                            Job = item.Job,
                            Machine_Code = item.Machine_Code,
                            Reveal_Name = item.Reveal_Name,
                            ApproveTime = item.ApproveTime,
                            Item_QUANTITY = x.Item_QUANTITY,
                            Name_EN = x.Name_EN,
                            PickUpID = item.ID,
                            Total = item.Total,
                        }).ToList();
                        rows.AddRange(detail);
                    }
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);


                }
                else if (reportType == 4)
                {
                    var column = new string[] { "Date", "Recieve_ID", "ApproveBy", "ID_Product","Reveal_Name", "ApproveTime", "Quantity", "Unit", "Unit_Price"};
                    var Do = context.Store_Dos.Where(x => x.Date.Date >= start.Date && x.Date.Date <= end && x.ApproveTime != null).ToList();
                    var rows = new List<ReportRecieve>();
                    foreach (var item in Do)
                    {
                        var detail = context.Store_Do_Datails.Where(x => x.DO_ID == item.ID).Select(x => new ReportRecieve
                        {
                            Date = item.Date,
                            Recieve_ID = item.ID,
                            ApproveBy = item.ApproveBy,
                            ID_Product = x.ID_Product,
                            Reveal_Name = item.InputBy,
                            ApproveTime = item.ApproveTime,
                            Quantity = x.Quantity,
                            Unit = x.Unit,
                            Unit_Price = x.Unit_Price
                        }).ToList();
                        rows.AddRange(detail);
                    }
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);
                }
                else if (reportType == 5)
                {
                    var column = new string[] { "TimeStamp", "SupplierID", "SupplierName", "AddressDetail", "Country", "Province", "District", "Sub_District", "PostCode", "Email", "FirstName", "LastName", "MobileNumber", "Fax", "Account_No" };
                    var rows = context.Store_AddSuppliers.Where(x => x.TimeStamp.Date >= start.Date && x.TimeStamp.Date <= end.Date && x.ApproveTime != null).Select(x => new
                    {
                        TimeStamp = x.TimeStamp,
                        SupplierID = x.SupplierID,
                        SupplierName = x.SupplierName,
                        AddressDetail = x.AddressDetail,
                        Country = x.Country,
                        Province = x.Province,
                        District = x.District,
                        Sub_District = x.Sub_District,
                        PostCode = x.PostCode,
                        Email = x.Email,
                        FirstName = x.FirstName1,
                        LastName = x.LastName1,
                        MobileNumber = x.MobileNumber1,
                        Fax = x.Fax1,
                        Account_No = x.Account_No
                    }).ToList();
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);
                }
                else if (reportType == 6)
                {
                    var column = new string[] { "TimeStamp", "SupplierID", "SupplierName", "AddressDetail", "Country", "Province", "District", "Sub_District", "PostCode", "Email", "FirstName", "LastName", "MobileNumber", "Fax", "Account_No" };
                    var Rawrows = new List<rawdataEditSP>();
                    var editsp = context.Store_EditSuppliers.Where(x => x.TimeStamp.Date >= start.Date && x.TimeStamp.Date <= end.Date && x.ApproveTime != null).ToList();
                    foreach (var item in editsp)
                    {
                        var row = new rawdataEditSP { dateTime = item.TimeStamp, store_Supplier = JsonConvert.DeserializeObject<Store_Supplier>(item.NewData) };
                        Rawrows.Add(row);
                    }
                    var rows = Rawrows.Select(x => new
                    {
                        TimeStamp = x.dateTime,
                        SupplierID = x.store_Supplier.SupplierID,
                        SupplierName = x.store_Supplier.SupplierName,
                        AddressDetail = x.store_Supplier.AddressDetail,
                        Country = x.store_Supplier.Country,
                        Province = x.store_Supplier.Province,
                        District = x.store_Supplier.District,
                        Sub_District = x.store_Supplier.Sub_District,
                        PostCode = x.store_Supplier.PostCode,
                        Email = x.store_Supplier.Email,
                        FirstName = x.store_Supplier.FirstName1,
                        LastName = x.store_Supplier.LastName1,
                        MobileNumber = x.store_Supplier.MobileNumber1,
                        Fax = x.store_Supplier.Fax1,
                        Account_No = x.store_Supplier.Account_No
                    }).ToList();
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);
                }
                else if (reportType == 7)
                {
                    var column = new string[] { "TimeStamp", "Apporve_Type", "Apporve_Type_Name", "Request_ID", "Request_Name", "Apporve_By_ID", "Apporve_By_Name", "Apporve_status", "Apporve_Text", "Apporve_date", "Reason", "Referent_EventID"};
                    var rows = context.Store_Approves.Where(x => x.Date.Date >= start.Date && x.Date.Date <= end.Date && x.Apporve_status != null).Select(x => new
                    {
                        TimeStamp = x.Date,
                        Apporve_Type = x.Apporve_Type,
                        Apporve_Type_Name = x.Apporve_Type_Name,
                        Request_ID = x.Request_ID,
                        Request_Name = x.Request_Name,
                        Apporve_By_ID = x.Apporve_By_ID,
                        Apporve_By_Name = x.Apporve_By_Name,
                        Apporve_status = x.Apporve_status,
                        Apporve_Text = x.Apporve_Text,
                        Apporve_date = x.Apporve_date,
                        Reason = x.Reason,
                        Referent_EventID = x.Referent_EventID
                    }).ToList();
                    var datalist = new { column, rows };
                    data = JsonConvert.SerializeObject(datalist, Formatting.Indented);
                }
            }
            return Json(data);
        }

        public class rawdataEditSP
        {
            public DateTime dateTime { get; set; }
            public Store_Supplier store_Supplier { get; set; }
        }
        public ActionResult Supplier()
        {
            var supplier = context.Store_Suppliers.Where(x => x.SupplierName != "delete").ToList();
            ViewData["supplier"] = supplier;
            return View();
        }

        public ActionResult AddSupplier(Store_Supplier _AddSupplier)
        {
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            Store_AddSupplier addSupplier = new Store_AddSupplier();
            addSupplier.TimeStamp = DateTime.Now;
            addSupplier.SupplierID = _AddSupplier.SupplierID;
            addSupplier.SupplierName = _AddSupplier.SupplierName;
            addSupplier.AddressDetail = _AddSupplier.AddressDetail;
            addSupplier.Country = _AddSupplier.Country;
            addSupplier.Province = _AddSupplier.Province;
            addSupplier.District = _AddSupplier.District;
            addSupplier.Sub_District = _AddSupplier.Sub_District;
            addSupplier.PostCode = _AddSupplier.PostCode;
            addSupplier.Email = _AddSupplier.Email;
            addSupplier.Email2 = _AddSupplier.Email2;
            addSupplier.FirstName1 = _AddSupplier.FirstName1;
            addSupplier.LastName1 = _AddSupplier.LastName1;
            addSupplier.MobileNumber1 = _AddSupplier.MobileNumber1;
            addSupplier.FirstName2 = _AddSupplier.FirstName2;
            addSupplier.LastName2 = _AddSupplier.LastName2;
            addSupplier.MobileNumber2 = _AddSupplier.MobileNumber2;
            addSupplier.PhoneNumber1 = _AddSupplier.PhoneNumber1;
            addSupplier.PhoneNumber2 = _AddSupplier.PhoneNumber2;
            addSupplier.Fax1 = _AddSupplier.Fax1;
            addSupplier.Fax2 = _AddSupplier.Fax2;
            addSupplier.Credit = _AddSupplier.Credit;
            addSupplier.Account_No = _AddSupplier.Account_No;
            addSupplier.Maker = user.BarCode;
            context.Store_AddSuppliers.Add(addSupplier);
            context.SaveChanges();
            Store_Approve _Approve = new Store_Approve();
            _Approve.Apporve_Type = 5;
            _Approve.Apporve_Type_Name = "AddSupplier";
            _Approve.Date = DateTime.Now;
            _Approve.Request_ID = user.ID;
            _Approve.Request_Name = user.Username;
            _Approve.Apporve_status = 0;
            _Approve.Apporve_Text = "wait Approve";
            _Approve.Referent_EventID = context.Store_AddSuppliers.Where(x => x.SupplierID == _AddSupplier.SupplierID).FirstOrDefault()?.ID;
            context.Store_Approves.Add(_Approve);
            context.SaveChanges();
            return RedirectToAction("Supplier");
        }

        public JsonResult findForEdit(int ID)
        {
            var supplier = context.Store_Suppliers.Where(x => x.ID == ID).FirstOrDefault();
            return Json(supplier);
        }

        public JsonResult EditSupplier(Store_Supplier _EditSupplier)
        {
            var status = "fail";
            try
            {
                var lastData = context.Store_Suppliers.FirstOrDefault(x => x.ID == _EditSupplier.ID);
                var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
                Store_EditSupplier editSupplier = new Store_EditSupplier();
                editSupplier.TimeStamp = DateTime.Now;
                editSupplier.SupplierID = _EditSupplier.ID;
                editSupplier.LastData = JsonConvert.SerializeObject(lastData);
                editSupplier.NewData = JsonConvert.SerializeObject(_EditSupplier);
                editSupplier.Maker = user.BarCode;
                context.Store_EditSuppliers.Add(editSupplier);
                context.SaveChanges();
                Store_Approve _Approve = new Store_Approve();
                _Approve.Apporve_Type = 6;
                _Approve.Apporve_Type_Name = "EditSupplier";
                _Approve.Date = DateTime.Now;
                _Approve.Request_ID = user.ID;
                _Approve.Request_Name = user.Username;
                _Approve.Apporve_status = 0;
                _Approve.Apporve_Text = "wait Approve";
                _Approve.Referent_EventID = editSupplier.ID;
                context.Store_Approves.Add(_Approve);
                context.SaveChanges();
                status = "Success";
            }
            catch
            {
            }

            return Json(status);
        }

        public ActionResult Recieve()
        {
            return View();
        }

        public JsonResult recieveDeatil(string barcode, int qty)
        {
            var alert = "fail";
            List<Store_Part> part = new List<Store_Part>();
            var checkList = HttpContext.Session.Get<List<Store_Part>>("ListQTY");
            var searchProduct = new Store_Part();

            if (checkList != null)
            {
                part = checkList;

            }
            if (!(barcode == null || qty == 0))
            {
                searchProduct = context.Store_Parts.Where(x => x.ID_Product == barcode).FirstOrDefault();
                if (searchProduct != null)
                {
                    searchProduct.QUANTITY = qty;
                    part.Add(searchProduct);
                    HttpContext.Session.Set<List<Store_Part>>("ListQTY", part);
                    alert = "success";
                }
            }
            return Json(new { alert = alert, searchProduct = searchProduct });
        }

        public JsonResult deleteItemInRecieve(string barcode)
        {
            var alert = "fail";
            var ListRecieve = HttpContext.Session.Get<List<Store_Part>>("ListQTY");
            if (!(ListRecieve == null || barcode == ""))
            {
                var splitBarcode = barcode.Split(":");
                var idproduct = splitBarcode[0];
                var qty = Convert.ToInt32(splitBarcode[1]);
                var itemDelete = ListRecieve.Where(x => x.ID_Product == idproduct && x.QUANTITY == qty).FirstOrDefault();
                ListRecieve.Remove(itemDelete);
                HttpContext.Session.Set<List<Store_Part>>("ListQTY", ListRecieve);
                alert = "Success";
            }
            return Json(alert);
        }

        public JsonResult SaveRecieve(string supplierID, string InputDate)
        {
            var alert = "fail";
            var ListRecieve = HttpContext.Session.Get<List<Store_Part>>("ListQTY");
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            var date = DateTime.Parse(InputDate);
            if (!(ListRecieve == null))
            {
                Store_Do store = new Store_Do();
                store.SupplierID = supplierID;
                store.Date = DateTime.Now;
                store.InputBy = user.Username;
                store.InputDate = date;
                context.Store_Dos.Add(store);
                context.SaveChanges();
                foreach (var item in ListRecieve)
                {
                    var part = context.Store_Parts.Where(x => x.ID_Product == item.ID_Product).FirstOrDefault();
                    Store_Do_Datail _Datail = new Store_Do_Datail();
                    _Datail.DO_ID = store.ID;
                    _Datail.ID_Product = item.ID_Product;
                    _Datail.Quantity = item.QUANTITY;
                    _Datail.Unit = item.UNIT;
                    _Datail.Unit_Price = item.UNIT_PRICE;
                    context.Store_Do_Datails.Add(_Datail);
                    context.SaveChanges();
                }
                Store_Approve approve = new Store_Approve();
                approve.Apporve_Type = 4;
                approve.Apporve_Type_Name = "Recieve";
                approve.Date = DateTime.Now;
                approve.Request_ID = user.ID;
                approve.Request_Name = user.Username;
                approve.Apporve_status = 0;
                approve.Apporve_Text = "waitApprove";
                approve.Referent_EventID = store.ID;
                context.Store_Approves.Add(approve);
                context.SaveChanges();
                alert = "Success";
                HttpContext.Session.Set<List<Store_Part>>("ListQTY", new List<Store_Part>());
            }
            return Json(alert);
        }
    }
}
