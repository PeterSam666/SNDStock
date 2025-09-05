using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SonodaSoftware.Data;
using SonodaSoftware.Services;
using SonodaSoftware.Services.JobServices;

namespace SonodaSoftware.Controllers
{
    public class JobController : Controller
    {
        private readonly IPartService _partService;

        public JobController(IPartService partService)
        {
            _partService = partService;
        }

        public IActionResult Recieve()
        {
            var jobName = _partService.GetAllJob();
            ViewData["jobName"] = jobName;
            return View();
        }

        public JsonResult AddJob(string jobname,string person)
        {
            Job_NameJob job_Name = new Job_NameJob();
            try
            {
                job_Name.StartAt = DateTime.Now;
                job_Name.JobName = jobname;
                job_Name.Status = 1;
                job_Name.ResponsiblePerson = person;
                _partService.AddJob(job_Name);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }
            return Json("Success");
        }

        public class PartSaveRequest
        {
            public List<Job_partLog> Parts { get; set; }
            public string Location { get; set; }
        }

        [HttpPost]
        public JsonResult SavePart([FromBody] PartSaveRequest request)
        {
            try
            {
                var user = HttpContext.Session.Get<User_UserBase>("UserLogin");
                request.Parts.ForEach(p => p.Reveal_Name = user.Username);
                _partService.SavePartList(request.Parts, request.Location);
                return Json(new { success = true, message = "บันทึกสำเร็จ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
            }
        }


        public IActionResult PartSearch()
        {
            return View();
        }

        public IActionResult Partsearchresult(string keyword, string searchBy)
        {
            var result = _partService.GetPartByKeyword(keyword, searchBy);
            var jobMap = _partService.GetJobNameMap();
            ViewData["jobMap"] = jobMap;
            ViewData["part"] = result;
            return View("PartSearch"); // กลับไปยังหน้า PartSearch พร้อมผลลัพธ์
        }

        [HttpPost]
        public JsonResult SaveEditpart(Job_Part_inStore EditPart)
        {
            try
            {
                var user = HttpContext.Session.Get<User_UserBase>("UserLogin");
                _partService.EditPart(EditPart,user);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Pickup()
        {
            var jobName = _partService.GetAllJob();
            ViewData["jobName"] = jobName;
            return View();
        }

        public JsonResult GetpartbyJob(int jobId)
        {
            var part = _partService.GetPartByJob(jobId);
            return Json(part);
        }

        public class PickupRequest
        {
            public List<Job_Part_inStore> PickupList { get; set; }
            public string Barcode { get; set; }
        }

        [HttpPost]
        public JsonResult PickupSave([FromBody] PickupRequest request)
        {
            try
            {
                var user = string.IsNullOrEmpty(request.Barcode)
                    ? HttpContext?.Session.Get<User_UserBase>("UserLogin")
                    : _partService.GetUser(request.Barcode);

                _partService.PickupParts(request.PickupList, user);

                return new JsonResult(new
                {
                    success = true,
                    message = "Pickup success"
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        public IActionResult Report()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Getreport(DateTime StartDate, DateTime EndDate,int ReportType)
        {
            var data = _partService.GetPartLogReport(StartDate, EndDate, ReportType);
            return Json(data);
        }

    }

}
