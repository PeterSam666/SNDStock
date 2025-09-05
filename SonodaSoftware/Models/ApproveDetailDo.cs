namespace SonodaSoftware.Models
{
    public class ApproveDetailDo
    {
        public DateOnly Date { get; set; }
        public string SupplierID { get; set; }
        public string InputBy { get; set; }
        public DateOnly? InputDate { get; set; }
        public string ID_Product  { get; set; }
        public string Name_product { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
        public double? Unit_Price { get; set; }
    }
}
