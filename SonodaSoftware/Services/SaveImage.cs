using SonodaSoftware.Data;

namespace SonodaSoftware.Services
{
    public class SaveImage
    {
        public string saveImageStore(IFormFile file)
        {
            var path = "";
            return path;
        }
        public string saveImageTool(IFormFile file)
        {
            var path = "";
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/imgtool", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                path = file.FileName; // บันทึกชื่อไฟล์ใน database
            }
            return path;
        }
    }
}
