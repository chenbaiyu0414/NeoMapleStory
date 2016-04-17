using System;

namespace NeoMapleStory.Core
{
    public static class DateUtiliy
    {
        private static readonly long FtUtOffset = 116444520000000000L;

        public static bool IsDst() => DateTime.Now.IsDaylightSavingTime();


        public static long GetFileTimestamp(long timeStampinMillis)
        {
            return GetFileTimestamp(timeStampinMillis, false);
        }

        public static long GetFileTimestamp(long timeStampinMillis, bool roundToMinutes)
        {
            if (IsDst())
            {
                timeStampinMillis -= 3600000L; //60 * 60 * 1000
            }
            timeStampinMillis += 14 * 60 * 60 * 1000;

            long time;
            if (roundToMinutes)
            {
                time = timeStampinMillis / 1000 / 60 * 600000000;
            }
            else {
                time = timeStampinMillis * 10000;
            }
            return time + FtUtOffset;
        }

        //public static long FT_UT_OFFSET = 116444592000000000;
        //public static long MAX_TIME = 150842304000000000;
        //public static long ZERO_TIME = 94354848000000000;
        //public static long PERMANENT = 150841440000000000;
        //public static long RENWU_TIME = 814568955L;
        //public static int QUESTFINISH_TIME = 1300000000;

        public static long GetTimeMilliseconds(this DateTime datetime) => (long)(datetime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;

        //public static long GetTime(int Kinds = 0)
        //{
        //    TimeSpan span = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
        //    long currenttimemillis = (long)span.TotalMilliseconds;

        //    switch (Kinds)
        //    {
        //        case -1:
        //            return MAX_TIME;
        //        case -2:
        //            return ZERO_TIME;
        //        case -3:
        //            return PERMANENT;
        //        case -5:
        //            return currenttimemillis * 10000 - RENWU_TIME;
        //    }
        //    return currenttimemillis * 10000 + FT_UT_OFFSET;
        //}

        //public static string GetTrueServerName(string Servername)
        //{
        //    return Servername.Substring(0, Servername.Length - 3);
        //}

        //public static long GetFileTimestamp()
        //{
        //    TimeSpan span = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
        //    long currenttimemillis = (long)span.TotalMilliseconds;
        //    return GetFileTimestamp(currenttimemillis, false);
        //}

        //public static long GetFileTimestamp(long timeStampinMillis, bool roundToMinutes)
        //{
        //    if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
        //    {
        //        timeStampinMillis -= 3600000;
        //    }
        //    long time;

        //    if (roundToMinutes)
        //    {
        //        time = timeStampinMillis / 1000L / 60L * 600000000L;
        //    }
        //    else {
        //        time = timeStampinMillis * 10000L;
        //    }
        //    return time + FT_UT_OFFSET;
        //}
    }
}
