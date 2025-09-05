using SonodaSoftware.Data;

namespace SonodaSoftware.Services.JobServices
{
    public interface IPartService
    {
        User_UserBase GetUser(string barcode);
        IEnumerable<Job_Part_inStore> GetAllPart();
        List<Job_Part_inStore> GetPartByJob(int job);
        List<Job_Part_inStore> GetPartByKeyword(string keyword,string SearchBy);
        Dictionary<int, string> GetJobNameMap();
        List<Job_NameJob> GetAllJob();
        void AddJob(Job_NameJob job_Name);
        void EditPart(Job_Part_inStore updatedPart, User_UserBase user);
        void SavePartList(List<Job_partLog> parts, string location);
        void PickupParts(List<Job_Part_inStore> parts, User_UserBase user);
        List<object> GetPartLogReport(DateTime startDate, DateTime endDate, int ReportType);
    }
}
