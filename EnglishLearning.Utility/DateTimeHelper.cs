using System;

namespace EnglishLearning.Utility
{
    public class DateTimeHelper
    {
        public static string GetStandardFormattedDateTimeString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetStandardTimeSpanString(TimeSpan timeSpan)
        {
            string value = timeSpan.ToString("c");

            var items = value.Split('.');

            if (items.Length == 2)
            {
                return $"{items[0]}.{(items[1].Length > 3 ? items[1].Substring(0,3) : items[1])}";
            }
            else
            {
                return value;
            }
        }

        public static string GetStandardTimeSpanString(double seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);

            return GetStandardTimeSpanString(ts);
        }
    }
}