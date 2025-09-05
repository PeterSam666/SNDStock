namespace SonodaSoftware.Models
{
    public class ReportRecieve
    {
        public DateTime Date {  get; set; }   
        public int Recieve_ID { get; set; }
        public string ID_Product {  get; set; }
        public string ApproveBy { get; set; }
        public string Job {  get; set; }
        public string Machine_Code { get; set; }
        public string Reveal_Name { get; set; }
        public DateTime? ApproveTime { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
        public double? Unit_Price { get; set; }
    }
}
