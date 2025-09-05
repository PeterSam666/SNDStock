using Microsoft.AspNetCore.Mvc;
using SonodaSoftware.Data;

namespace SonodaSoftware.Controllers
{
    public class ReportController : Controller
    {
        SND_DBContext context = new SND_DBContext();
        public JsonResult Addpart(DateTime StartDate, DateTime EndDate)
        {
            var tableDetail = context.Store_AddParts.Where(x => x.TimeStamp >= StartDate && x.TimeStamp <= EndDate).ToList();
            return Json(tableDetail);
        }
        public JsonResult Editpart(DateTime StartDate, DateTime EndDate)
        {
            return Json("");
        }
        public JsonResult DeletePart(DateTime StartDate, DateTime EndDate)
        {
            return Json("");
        }
        public JsonResult PickUp(DateTime StartDate, DateTime EndDate)
        {
            return Json("");
        }
        public JsonResult Do(DateTime StartDate, DateTime EndDate)
        {
            return Json("");
        }
        public JsonResult AddSupplier(DateTime StartDate, DateTime EndDate)
        {
            return Json("");
        }
        public JsonResult EditSupplier(DateTime StartDate, DateTime EndDate)
        {
            return Json("");
        }
        public JsonResult AddQTY()
        {
            return Json("");
        }
    }
}
