using System;

namespace oDeskNotifier
{
    class UnixTime
    {
        static DateTime unixEpoch;
        static UnixTime()
        {
            unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public static int Now { get { return DateTimeToUnixTimestamp(DateTime.UtcNow); } }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return unixEpoch.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public static int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (int)((dateTime.ToUniversalTime() - unixEpoch).TotalSeconds);
        }
    }
}
