namespace SonodaSoftware.Models
{
    public class reportPickUp
    {
        public DateTime Date {  get; set; }
        public string Job {  get; set; }
        public string Machine_Code { get; set; }
        public string Reveal_Name { get; set; }
        public string ID_Product { get; set; }
        public string Name_EN { get; set; }
        public int Item_QUANTITY { get; set; }
        public int PickUpID { get; set; }
        public int Total { get; set; }
        public string ApproveBy { get; set; }
        public DateTime? ApproveTime { get; set; }
    }
}
