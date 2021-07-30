using System;

namespace GamesBucket.App.Helpers
{
    public static class TimeConverter
    {
        /// <summary>
        /// Converts UnixTimeStamp date format to DateTime date format
        /// </summary>
        /// <param name="unixTimeStampStr"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(this string unixTimeStampStr)
        {
            // Unix timestamp is seconds past epoch
            if (Double.TryParse(unixTimeStampStr, out double unixTimeStamp))
            {
                DateTime dtDateTime = new 
                    DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
                return dtDateTime;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Convert DateTime to Unix Epoch format
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// References:
        /// http://esqsoft.com/javascript_examples/date-to-epoch.htm
        public static string ToUnixTimeStamp(this DateTime time)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((time.ToUniversalTime() - epoch).TotalSeconds).ToString() + "000";
        }
    }
}