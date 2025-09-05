using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SonodaSoftware.Data;
using SonodaSoftware.Models;
using SonodaSoftware.Services;
using System.Collections.Generic;

namespace SonodaSoftware.Controllers
{
    [SessionFilter]
    public class ToolController : Controller
    {
        SaveImage saveImage = new SaveImage();
        SND_DBContext context = new SND_DBContext();
        public IActionResult Index()
        {
            var tool = context.Tools.ToList();
            ViewData["tool"] = tool;
            return View();
        }

        public JsonResult SaveAndclearToolBorrow(string[] IdTool)
        {
            List<Tool> tool = new List<Tool>(); 
            if(IdTool != null)
            {
                foreach (var item in IdTool)
                {
                    var id = Convert.ToInt32(item);
                    var itemTool = context.Tools.Where(x => x.ID == id).FirstOrDefault();
                    tool.Add(itemTool);
                }
            }
            HttpContext.Session.Set<List<Tool>>("ToolToBorrow", tool);
            return Json("");
        }

        public IActionResult Borrow()
        {
            var checkBorrow = HttpContext.Session.Get<List<Tool>>("ToolToBorrow");
            var tool = context.Tools.ToList();
            ViewData["tool"] = tool;
            ViewData["checkBorrow"] = checkBorrow;
            return View();
        }

        public IActionResult BorrowDetailSave(string Job, string UserBorrow, List<Tool_BorrowDetail> borrowDetails)
        {
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            var borrows = borrowDetails.Where(x => x.Quantity != 0 && x.Tool != null).ToList();
            var findUserBorrow = context.User_UserBases.FirstOrDefault(x => x.BarCode.Contains(UserBorrow));

            try
            {
                if (borrows.Count != 0)
                {
                    if (findUserBorrow != null)
                    {
                        // 1. สร้างรายการ Borrow หลัก
                        Tool_Borrow borrow = new Tool_Borrow
                        {
                            Date = DateTime.Now,
                            Job = Job,
                            BorrowBy = user.BarCode,
                            UserID = findUserBorrow.BarCode,
                            Reveral_Name = findUserBorrow.Username,
                            ApproveBy = user.BarCode,
                            Description = "",
                            Total = borrows.Sum(x => x.Quantity)
                        };
                        context.Tool_Borrows.Add(borrow);
                        context.SaveChanges(); // Save เพื่อให้ได้ BorrowID

                        // 2. สร้างรายการ BorrowDetail และลดสต๊อก Tool
                        foreach (var borrowDetail in borrows)
                        {
                            var tool = context.Tools.FirstOrDefault(x => x.ID.ToString() == borrowDetail.Tool);
                            if (tool != null)
                            {
                                // ลดจำนวน
                                tool.Quantity = (byte)Math.Max(0, tool.Quantity - borrowDetail.Quantity ?? 0);

                                // บันทึกรายละเอียด
                                borrowDetail.Unit = tool.Unit;
                                borrowDetail.BorrowID = borrow.ID;
                                context.Tool_BorrowDetails.Add(borrowDetail);
                            }
                        }

                        context.SaveChanges();
                        TempData["result"] = "success";
                    }
                    else
                    {
                        TempData["result"] = "No User";
                        return RedirectToAction("Borrow");
                    }
                }
            }
            catch
            {
                TempData["result"] = "fail";
                return RedirectToAction("Borrow");
            }

            return RedirectToAction("Index");
        }


        public IActionResult Return()
        {
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            if(user != null)
            {
                var StaffUser = context.User_Accesses.Where(x => x.IDUser == user.BarCode&& x.Tool.Contains("ManageBorrow")).FirstOrDefault();
                if(StaffUser!= null)
                {
                    var borrow = context.Tool_Borrows.Where(x => x.BorrowBy == user.BarCode).ToList();
                    List<DetailForReturn> detailForReturn = new List<DetailForReturn>();
                    foreach (var item in borrow)
                    {
                        var details = context.Tool_BorrowDetails.Where(x => x.BorrowID == item.ID && x.ReturnAt == null).ToList();
                        foreach (var detail in details)
                        {
                            DetailForReturn forReturn = new DetailForReturn();
                            forReturn.BorrowId = item.ID;
                            forReturn.DetailId = detail.ID;
                            forReturn.Date = item.Date;
                            forReturn.Job = item.Job;
                            forReturn.Tool = context.Tools.FirstOrDefault(x => x.ID.ToString() == detail.Tool).Name_ENG;
                            forReturn.Quantity = detail.Quantity;
                            forReturn.Unit = detail.Unit;
                            detailForReturn.Add(forReturn);
                        }
                    }
                    ViewData["detail"] = detailForReturn;
                }
                else
                {
                    var borrow = context.Tool_Borrows.Where(x => x.UserID == user.BarCode).ToList();
                    List<DetailForReturn> detailForReturn = new List<DetailForReturn>();
                    foreach (var item in borrow)
                    {
                        var details = context.Tool_BorrowDetails.Where(x => x.BorrowID == item.ID && x.ReturnAt == null).ToList();
                        foreach (var detail in details)
                        {
                            DetailForReturn forReturn = new DetailForReturn();
                            forReturn.BorrowId = item.ID;
                            forReturn.DetailId = detail.ID;
                            forReturn.Date = item.Date;
                            forReturn.Job = item.Job;
                            forReturn.Tool = context.Tools.FirstOrDefault(x => x.ID.ToString() == detail.Tool).Name_ENG;
                            forReturn.Quantity = detail.Quantity;
                            forReturn.Unit = detail.Unit;
                            detailForReturn.Add(forReturn);
                        }
                    }
                    ViewData["detail"] = detailForReturn;
                }
            }
            return View();
        }

        public IActionResult returnsave(DetailForReturn detailForReturn)
        {
            var borrow = context.Tool_BorrowDetails.FirstOrDefault(x => x.ID == detailForReturn.DetailId);
            if (borrow == null)
            {
                TempData["result"] = "ไม่พบข้อมูลที่ต้องการคืน";
                return RedirectToAction("Return");
            }

            // ตรวจว่าคืนไปแล้วหรือยัง
            if (borrow.ReturnAt != null)
            {
                TempData["result"] = "รายการนี้ถูกคืนไปแล้ว";
                return RedirectToAction("Return");
            }

            borrow.ReturnAt = DateTime.Now;
            context.Tool_BorrowDetails.Update(borrow);

            // คืนจำนวนเข้า Tools
            var tool = context.Tools.FirstOrDefault(x => x.ID.ToString() == borrow.Tool);
            if (tool != null)
            {
                tool.Quantity = Math.Max(0, tool.Quantity + borrow.Quantity ?? 0);
                context.Tools.Update(tool);
            }

            context.SaveChanges();
            TempData["result"] = "คืนสำเร็จ";
            return RedirectToAction("Return");
        }


        public IActionResult EditTool()
        {
            var checkBarcode = context.Tools.OrderByDescending(x => x.ID).FirstOrDefault()?.ID ?? 0;
            var runbarcode = Convert.ToInt32(checkBarcode) + 1;
            var tool = context.Tools.OrderBy(x => x.ID).ToList();
            ViewData["tool"] = tool;
            ViewData["runbarcode"] = runbarcode;
            return View();
        }

        public ActionResult AddTool(Tool tool, IFormFile file)
        {
            var addpicture = saveImage.saveImageTool(file);
            var LastTool = context.Tools.Where(x => x.ID == tool.ID).FirstOrDefault();
            var check = (LastTool == null) ? true : false;
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            if (check)
            {
                Tool createTool = new Tool();
                createTool.Name_ENG = tool.Name_ENG;
                createTool.Name_TH = tool.Name_TH;
                createTool.Quantity = tool.Quantity;
                createTool.Unit = tool.Unit;
                createTool.Location = tool.Location;
                createTool.Supplier = tool.Supplier;
                createTool.CreateOn = DateTime.Now;
                createTool.Picture = addpicture;
                Tool_Event toolEvent = new Tool_Event();
                toolEvent.DateTime = DateTime.Now;
                toolEvent.ToolID = tool.ID;
                toolEvent.LastQuantity = LastTool?.Quantity??0;
                toolEvent.NewQuantity = tool.Quantity;
                toolEvent.Event = "AddTool";
                toolEvent.EditBy = user.BarCode;
                context.Tools.Add(createTool);
                context.SaveChanges();
                context.Tool_Events.Add(toolEvent);
                context.SaveChanges();
                TempData["AlertMessage"] = "Success";
                return RedirectToAction("EditTool");
            }
            TempData["AlertMessage"] = "fail";
            return RedirectToAction("EditTool");
        }

        public ActionResult EditDetailTool(Tool tool, IFormFile file)
        {
            var addpicture = saveImage.saveImageTool(file);
            var LastTool = context.Tools.Where(x => x.ID == tool.ID).FirstOrDefault();
            var check = (LastTool != null) ? true : false;
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            if (check)
            {
                LastTool.Name_ENG = tool.Name_ENG;
                LastTool.Name_TH = tool.Name_TH;
                LastTool.Quantity = tool.Quantity;
                LastTool.Unit = tool.Unit;
                LastTool.Location = tool.Location;
                LastTool.Supplier = tool.Supplier;
                LastTool.CreateOn = DateTime.Now;
                LastTool.Picture = addpicture;
                Tool_Event toolEvent = new Tool_Event();
                toolEvent.DateTime = DateTime.Now;
                toolEvent.ToolID = tool.ID;
                toolEvent.LastQuantity = LastTool?.Quantity ?? 0;
                toolEvent.NewQuantity = tool.Quantity;
                toolEvent.Event = "EditDetailTool";
                toolEvent.EditBy = user.BarCode;
                context.Tools.Update(LastTool);
                context.SaveChanges();
                context.Tool_Events.Add(toolEvent);
                context.SaveChanges();
                TempData["AlertMessage"] = "Success";
                return RedirectToAction("EditTool");
            }
            TempData["AlertMessage"] = "fail";
            return RedirectToAction("EditTool");
        }

        public ActionResult DeleteTool(Tool tool)
        {
            var LastTool = context.Tools.Where(x => x.ID == tool.ID).FirstOrDefault();
            var check = (LastTool == null) ? true : false;
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            if (check)
            {
                TempData["AlertMessage"] = "fail";
                return RedirectToAction("EditTool");
            }
            Tool_Event toolEvent = new Tool_Event();
            toolEvent.DateTime = DateTime.Now;
            toolEvent.ToolID = tool.ID;
            toolEvent.LastQuantity = LastTool?.Quantity ?? 0;
            toolEvent.NewQuantity = 0;
            toolEvent.Event = "DeleteTool";
            toolEvent.EditBy = user.BarCode;
            context.Remove(LastTool);
            context.Tool_Events.Add(toolEvent);
            context.SaveChanges();
            TempData["AlertMessage"] = "Success";
            return RedirectToAction("EditTool");
        }

        public ActionResult plusTool(Tool tool)
        {
            var LastTool = context.Tools.Where(x => x.ID == tool.ID).FirstOrDefault();
            var check = (LastTool != null) ? true : false;
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            if (check)
            {
                Tool_Event toolEvent = new Tool_Event();
                toolEvent.DateTime = DateTime.Now;
                toolEvent.ToolID = tool.ID;
                toolEvent.LastQuantity = LastTool?.Quantity ?? 0;
                toolEvent.Event = "plusTool";
                toolEvent.EditBy = user.BarCode;
                LastTool.Quantity += tool.Quantity;
                toolEvent.NewQuantity = LastTool.Quantity;
                context.Tools.Update(LastTool);
                context.Tool_Events.Add(toolEvent);
                context.SaveChanges();
                TempData["AlertMessage"] = "Success";
            }
            else
            {
                TempData["AlertMessage"] = "fail";
            }
            return RedirectToAction("EditTool");
        }

        public ActionResult DownTool(Tool tool)
        {
            var LastTool = context.Tools.Where(x => x.ID == tool.ID).FirstOrDefault();
            var check = (LastTool != null) ? true : false;
            var user = HttpContext?.Session.Get<User_UserBase>("UserLogin");
            if (check)
            {
                if(LastTool.Quantity >= tool.Quantity)
                {
                    Tool_Event toolEvent = new Tool_Event();
                    toolEvent.DateTime = DateTime.Now;
                    toolEvent.ToolID = tool.ID;
                    toolEvent.LastQuantity = LastTool?.Quantity ?? 0;
                    toolEvent.Event = "DownTool";
                    toolEvent.EditBy = user.BarCode;
                    LastTool.Quantity -= tool.Quantity;
                    toolEvent.NewQuantity = LastTool.Quantity;
                    context.Tools.Update(LastTool);
                    context.Tool_Events.Add(toolEvent);
                    context.SaveChanges();
                    TempData["AlertMessage"] = "Success";
                }
                else
                {
                    TempData["AlertMessage"] = $"Please try Quantity Less than {LastTool.Quantity}";
                }
                
            }
            else
            {
                TempData["AlertMessage"] = "fail";
            }
            return RedirectToAction("EditTool");
        }

        public IActionResult Approve()
        {
            return View();
        }

        public IActionResult History()
        {
            var historyModel = new List<HistoryModel>();
            var datefilter = DateTime.Now;
            var Borrow = (HttpContext.Session.Get<List<Tool_Borrow>>("reportTool") == null)?context.Tool_Borrows.Where(x => x.Date.Date == datefilter.Date).ToList(): HttpContext.Session.Get<List<Tool_Borrow>>("reportTool");
            foreach (var borrow in Borrow)
            {
                var Detail = context.Tool_BorrowDetails.Where(x => x.BorrowID == borrow.ID).ToList();
                foreach (var item in Detail)
                {
                    HistoryModel history = new HistoryModel();
                    history.DateTime = borrow.Date;
                    history.UserBorrow = borrow.Reveral_Name;
                    history.Job = borrow.Job;
                    history.Tool = context.Tools.FirstOrDefault(x => x.ID.ToString() == item.Tool).Name_ENG;
                    history.Quantity = item.Quantity;
                    history.Unit = item.Unit;
                    history.ReturnAt = item.ReturnAt;
                    history.status = (borrow.Status == 1) ? "คืนแล้ว" : "ยังไม่คืน";
                    historyModel.Add(history);
                }
            }
            ViewData["historyModel"] = historyModel;
            return View();
        }

        public ActionResult historyFilterDate(DateTime startDate,DateTime endDate)
        {
            var checkNullStart = new DateTime(1, 1, 0001);
            var checkNullEnd = new DateTime(1, 1, 0001);
            if (!(startDate == checkNullStart || endDate == checkNullEnd))
            {
                var report = context.Tool_Borrows.Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date).ToList();
                HttpContext.Session.Set<List<Tool_Borrow>>("reportTool", report);
            }
            return RedirectToAction("History");
        }

        public IActionResult ViewToolBorrow()
        {
            var historyModel = new List<HistoryModel>();
            var borrowDetail = context.Tool_BorrowDetails.Where(x => x.ReturnAt == null).ToList();
            foreach (var IDBorrow in borrowDetail.GroupBy(x => x.BorrowID).Select(x => x.Key).ToList())
            {
                var borrow = context.Tool_Borrows.FirstOrDefault(x => x.ID == IDBorrow);
                foreach (var item in borrowDetail.Where(x => x.ID == IDBorrow))
                {
                    HistoryModel history = new HistoryModel();
                    history.DateTime = borrow.Date;
                    history.UserBorrow = borrow.Reveral_Name;
                    history.Job = borrow.Job;
                    history.Tool = context.Tools.FirstOrDefault(x => x.ID.ToString() == item.Tool).Name_ENG;
                    history.Quantity = item.Quantity;
                    history.Unit = item.Unit;
                    historyModel.Add(history);
                }
            }
            ViewData["historyModel"] = historyModel;
            return View();
        }

    }
}
