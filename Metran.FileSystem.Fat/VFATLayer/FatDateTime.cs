using System;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Converts a System.DateTime to and from the FAT date and time formats
    /// </summary>
    public static class FatDateTime
    {
        private const int ReferenceYear = 1980;

        private const int YearDeltaOffset = 0;
        private const int YearDeltaLength = 7;

        private const int MonthOffset = 7;
        private const int MonthLength = 4;

        private const int DayOffset = 11;
        private const int DayLength = 5;

        private const int HourOffset = 0;
        private const int HourLength = 5;

        private const int MinuteOffset = 5;
        private const int MinuteLength = 6;

        private const int DoublesecondOffset = 11;
        private const int DoublesecondLength = 5;

        public static DateTime Pack(ushort date, ushort time, byte millisecondsTenths)
        {
            // just according to the spec...

            var year = ReferenceYear + Utils.ExtractBits(date, YearDeltaOffset, YearDeltaLength);
            int day = Utils.ExtractBits(date, DayOffset, DayLength);
            int month = Utils.ExtractBits(date, MonthOffset, MonthLength);

            int hour = Utils.ExtractBits(time, HourOffset, HourLength);
            int minute = Utils.ExtractBits(time, MinuteOffset, MinuteLength);
            var second = 2*Utils.ExtractBits(time, DoublesecondOffset, DoublesecondLength);

            var millisecond = 10*millisecondsTenths;
            while (millisecond >= 1000)
            {
                // move thousands of milliseconds to the seconds...

                millisecond -= 1000;
                second++;
            }

            var dt = new DateTime(
                year,
                month,
                day,
                hour,
                minute,
                second,
                millisecond);

            return dt;
        }

        public static DateTime Pack(ushort date, ushort time)
        {
            return Pack(date, time, 0);
        }

        public static DateTime Pack(ushort date)
        {
            return Pack(date, 0, 0);
        }

        public static void Unpack(DateTime dt, out ushort date, out ushort time, out byte millisecondsTenths)
        {
            // just according to the spec...

            var yearDelta = dt.Year - ReferenceYear;

            date = 0;
            date = Utils.InjectBits(date, YearDeltaOffset, YearDeltaLength, (ushort) yearDelta);
            date = Utils.InjectBits(date, MonthOffset, MonthLength, (ushort) dt.Month);
            date = Utils.InjectBits(date, DayOffset, DayLength, (ushort) dt.Day);

            var doubleseconds = dt.Second/2;

            time = 0;
            time = Utils.InjectBits(time, HourOffset, HourLength, (ushort) dt.Hour);
            time = Utils.InjectBits(time, MinuteOffset, MinuteLength, (ushort) dt.Minute);
            time = Utils.InjectBits(time, DoublesecondOffset, DoublesecondLength, (ushort) doubleseconds);

            var millisecond = dt.Millisecond + (dt.Second%2)*1000;

            millisecondsTenths = (byte) (millisecond/10);
        }

        public static void Unpack(DateTime dt, out ushort date, out ushort time)
        {
            byte millisecondsTenths;
            Unpack(dt, out date, out time, out millisecondsTenths);
        }

        public static void Unpack(DateTime dt, out ushort date)
        {
            ushort time;
            byte millisecondsTenths;
            Unpack(dt, out date, out time, out millisecondsTenths);
        }
    }
}