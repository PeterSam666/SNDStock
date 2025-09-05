using System.ComponentModel.DataAnnotations;

namespace SonodaSoftware.Models
{
    public class DetailForReturn
    {
        public DateTime Date { get; set; }
        public int BorrowId { get; set; }
        public int DetailId { get; set; }
        public string Job { get; set; }
        public string Tool { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
    }
}
