namespace SonodaSoftware.Services.StoreServices
{
    public static class DateHelper
    {
        public static DateTime ConvertDate(string date)
        {
            var parts = date.Split('/');
            return new DateTime(
                Convert.ToInt32(parts[2]),
                Convert.ToInt32(parts[1]),
                Convert.ToInt32(parts[0])
            );
        }
    }

}
