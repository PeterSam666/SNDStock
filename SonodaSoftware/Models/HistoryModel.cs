namespace SonodaSoftware.Models
{
    public class HistoryModel
    {
        public DateTime DateTime { get; set; }
        public string UserBorrow {  get; set; }
        public string Job { get; set; }
        public string Tool { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime? ReturnAt { get; set; }
        public string status { get; set; }
    }
}
