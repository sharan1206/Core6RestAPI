using System.Globalization;
using WesternRestApiCoreInMemory.Models;

namespace WesternRestApiCoreInMemory.Utils
{
    public static class HelperClass
    {

        /// <summary>
        /// Converts Unix timestamp to Utc
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }

        /// <summary>
        /// Converts Utc timestamp to Unix
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static uint ToUnixTime(DateTime dateTime)
        {
            return (uint)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }


        /// <summary>
        /// Returns true, if input parameter value is greater than now.
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static bool IsActiveGuid(double unixTime)
        {
            DateTime guidCreatedUtcTime = HelperClass.UnixTimeToDateTime(unixTime);
            DateTime currentUtc = DateTime.UtcNow;           

            return (guidCreatedUtcTime > currentUtc) ? true : false;            
        }


        /// <summary>
        /// Returns true, if input parameter value is greater than now.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsValidExpiryDateTime(string text)
        {
            DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double seconds = 0;
            if (!string.IsNullOrEmpty(text) && text.Trim().Length > 0)
            {
                seconds = double.Parse(text, CultureInfo.InvariantCulture);
                DateTime providedUnixDate = Epoch.AddSeconds(seconds);

                return (DateTime.UtcNow < providedUnixDate)? true : false;
            }
            return false;
        }
    }
}
