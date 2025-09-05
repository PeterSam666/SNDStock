using Microsoft.EntityFrameworkCore;
using SonodaSoftware.Data;

namespace SonodaSoftware.Services.JobServices
{
    public class PartService : IPartService
    {
        private readonly SND_DBContext _context;


        public PartService(SND_DBContext context)
        {
            _context = context;
        }

        public User_UserBase GetUser(string barcode)
        {
            var findUser = _context.User_UserBases.Where(x => x.BarCode.Contains(barcode)).ToList();
            var user = findUser.Where(x => x.BarCode.Split('-')[0].Contains(barcode)).FirstOrDefault();
            return user;
        }

        public IEnumerable<Job_Part_inStore> GetAllPart()
        {
            return _context.Job_Part_inStores.ToList();
        }

        public List<Job_Part_inStore> GetPartByJob(int job)
        {
            return _context.Job_Part_inStores.Where(x => x.JobID == job).ToList();
        }

        public List<Job_Part_inStore> GetPartByKeyword(string keyword, string SearchBy)
        {
            List<Job_Part_inStore> _Part_InStores = new List<Job_Part_inStore>();
            switch (SearchBy)
            {
                case "Barcode":
                    _Part_InStores = _context.Job_Part_inStores.Where(x => x.Barcode.Contains(keyword)).ToList();
                    break;
                case "PartNameEng":
                    _Part_InStores = _context.Job_Part_inStores.Where(x => x.PartNameEng.Contains(keyword)).ToList();
                    break;
                case "PartNameThai":
                    _Part_InStores = _context.Job_Part_inStores.Where(x => x.PartNameThai.Contains(keyword)).ToList();
                    break;
                case "Job":
                    var job = _context.Job_NameJobs.Where(x => x.JobName.Contains(keyword)).ToList();
                    int i = job.Count;
                    while (i != 0)
                    {
                        _Part_InStores.AddRange(_context.Job_Part_inStores.Where(x => x.JobID == job[i - 1].ID).ToList());
                        i--;
                    }
                    break;
                default:
                    break;
            }
            return _Part_InStores;
        }

        public Dictionary<int, string> GetJobNameMap()
        {
            var jobMap = _context.Job_NameJobs
                        .ToDictionary(j => j.ID, j => j.JobName);
            return jobMap;
        }

        public void EditPart(Job_Part_inStore updatedPart, User_UserBase user)
        {
            Job_partLog log = new Job_partLog();
            log.JobID = updatedPart.JobID;
            log.PONo = updatedPart.PONo;
            log.Quantity = updatedPart.Quantity;
            log.PartNameEng = updatedPart.PartNameEng;
            log.Barcode = updatedPart.Barcode;
            log.EventType = 3;
            log.PartNameThai = updatedPart.PartNameThai;
            log.DateTime = DateTime.Now;
            log.PRNo = updatedPart.PrNo;
            log.Reveal_Name = user.Username;
            _context.Job_partLogs.Add(log);

            var existingPart = _context.Job_Part_inStores.FirstOrDefault(p => p.ID == updatedPart.ID);
            if (existingPart == null)
                throw new Exception("ไม่พบ Part ที่ต้องการแก้ไข");

            // อัปเดตค่า
            existingPart.Barcode = updatedPart.Barcode;
            existingPart.PartNameEng = updatedPart.PartNameEng;
            existingPart.PartNameThai = updatedPart.PartNameThai;
            existingPart.JobID = updatedPart.JobID;
            existingPart.Quantity = updatedPart.Quantity;
            existingPart.Location = updatedPart.Location;
            existingPart.PrNo = updatedPart.PrNo;
            existingPart.PONo = updatedPart.PONo;
            existingPart.ModifyOn = DateTime.Now;

            _context.SaveChanges();
        }


        public void AddJob(Job_NameJob job_Name)
        {
            _context.Job_NameJobs.Add(job_Name);
            _context.SaveChanges();
        }

        public void SavePartList(List<Job_partLog> parts, string location)
        {
            if (parts == null || !parts.Any())
                throw new ArgumentException("ไม่มีข้อมูลที่ส่งมา");

            foreach (var part in parts)
            {
                part.DateTime = DateTime.Now;
                part.EventType = 1;
                part.Reveal_Name = part.Reveal_Name ?? "System";
                part.Description = "Recieve";

                _context.Job_partLogs.Add(part);

                var inStore = new Job_Part_inStore
                {
                    ID = 0,
                    Barcode = part.Barcode,
                    PartNameEng = part.PartNameEng,
                    PartNameThai = part.PartNameThai,
                    JobID = part.JobID,
                    Quantity = part.Quantity,
                    Location = location ?? "ไม่ระบุ",
                    CreateOn = DateTime.Now,
                    ModifyOn = DateTime.Now,
                    PrNo = part.PRNo,
                    PONo = part.PONo
                };

                _context.Job_Part_inStores.Add(inStore);
            }

            _context.SaveChanges();
        }

        public List<Job_NameJob> GetAllJob()
        {
            var jobName = _context.Job_NameJobs.ToList();
            return jobName;
        }

        public void PickupParts(List<Job_Part_inStore> parts, User_UserBase user)
        {
            if (parts == null || parts.Count == 0)
                throw new ArgumentException("ไม่มีรายการ Part สำหรับ Pickup");

            foreach (var part in parts)
            {
                var existingPart = _context.Job_Part_inStores.FirstOrDefault(p => p.ID == part.ID);
                if (existingPart != null)
                {
                    // อัปเดตข้อมูลในสโตร์
                    existingPart.ModifyOn = DateTime.Now;
                    existingPart.Quantity = (existingPart.Quantity ?? 0) - (part.Quantity ?? 0);

                    if (existingPart.Quantity < 0)
                        throw new InvalidOperationException($"Part {existingPart.Barcode} มี Quantity ไม่พอ");

                    // ➕ เพิ่ม Log
                    var log = new Job_partLog
                    {
                        DateTime = DateTime.Now,
                        Barcode = existingPart.Barcode,
                        PartNameEng = existingPart.PartNameEng,
                        PartNameThai = existingPart.PartNameThai,
                        JobID = existingPart.JobID,
                        EventType = 2, // 2 = Pickup
                        Quantity = existingPart.Quantity,
                        PRNo = existingPart.PrNo,
                        PONo = existingPart.PONo,
                        Description = "Pickup",
                        Reveal_Name = user.Username // หรือดึงจาก user ปัจจุบันถ้ามี
                    };

                    _context.Job_partLogs.Add(log);
                }
                else
                {
                    // ถ้ายังไม่มีให้เพิ่มใหม่
                    part.CreateOn = DateTime.Now;
                    part.ModifyOn = DateTime.Now;
                    _context.Job_Part_inStores.Add(part);
                }
            }

            _context.SaveChanges();
        }

        public List<object> GetPartLogReport(DateTime startDate, DateTime endDate, int ReportType)
        {
            DateTime start = startDate.Date;
            DateTime end = endDate.Date.AddDays(1);

            List<object> report = _context.Job_partLogs.Where(x => x.DateTime.Date >= startDate.Date && x.DateTime.Date <= endDate.Date && x.EventType == ReportType)
                .AsEnumerable().Select(log => (object)new
                {
                    DateTime = log.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Barcode = log.Barcode,
                    PartNameEng = log.PartNameEng,
                    PartNameThai = log.PartNameThai,
                    Job = (log.JobID != 0) ? _context.Job_NameJobs.FirstOrDefault(x => x.ID == log.JobID).JobName : "ไม่พบข้อมูล",      // 👈 แก้จาก job?.JobName
                    Quantity = log.Quantity,
                    Description = log.Description,
                    Reveal_Name = log.Reveal_Name
                }).ToList();

            return report;
        }
    }
}
