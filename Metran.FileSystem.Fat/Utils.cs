namespace Metran.FileSystem.Fat
{
    /// <summary>
    /// Provides internal clients with different auxiliary functions
    /// </summary>
    internal static class Utils
    {
        public const int ClusterNotAllocated = 0;

        public static int PackToInt32(ushort highWord, ushort lowWord)
        {
            return highWord << 16 | lowWord;
        }

        public static void Unpack(int value, out ushort highWord, out ushort lowWord)
        {
            lowWord = (ushort) (value & 0x0000FFFF);
            highWord = (ushort) (value >> 16);
        }

        public static uint PackToUInt32(ushort highWord, ushort lowWord)
        {
            return (uint) (highWord << 16 | lowWord);
        }

        public static void Unpack(uint value, out ushort highWord, out ushort lowWord)
        {
            lowWord = (ushort) (value & 0x0000FFFF);
            highWord = (ushort) (value >> 16);
        }

        public static byte RotateRight(byte value, int count)
        {
            return (byte) (value << (8 - count) | (value >> count));
        }

        public static ushort SetBit(ushort value, int msbBasedBit)
        {
            var settingMask = (ushort) (0x8000 >> msbBasedBit);

            return (ushort) (value | settingMask);
        }

        public static ushort ClearBit(ushort value, int msbBasedBit)
        {
            var clearingMask = (ushort) (~(0x8000 >> msbBasedBit));

            return (ushort) (value & clearingMask);
        }

        public static ushort ExtractBits(ushort value, int startBit, int length)
        {
            // 1. create a mask that suppresses the high bits of the value (from 0 to startBit)
            var highBitsSuppressionMask = (ushort) (0xFFFF >> startBit);

            // 2. apply the mask to the value
            var tempResult = (ushort) (value & highBitsSuppressionMask);

            // 3. shift the temp result right on (16 - (startBit+length)) bits
            // suppressing the low bits of the value (from (startBit+length) to 16)
            var result = (ushort) (tempResult >> (16 - (startBit + length)));

            return result;
        }

        public static ushort InjectBits(ushort targetValue, int startBit, int length, ushort value)
        {
            // 1. suppress the original bits in the target value (from startBit to (startBit+length))
            var tempResult = targetValue;
            for (var i = 0; i < length; i++)
            {
                tempResult = ClearBit(tempResult, startBit + i);
            }

            // 2. suppress the unused bits in the value (leave the low "length" bits there)
            var adjustingMask = (ushort) (0xFFFF >> (16 - length));
            var adjustedValue = (ushort) (value & adjustingMask);

            // 3. shift the adjusted value left on (16-(startBit+length)) bits
            var shiftedAdjustedValue = (ushort) (adjustedValue << (16 - (startBit + length)));

            // 4. add the shifted adjusted value to the temp result
            var result = (ushort) (tempResult | shiftedAdjustedValue);

            return result;
        }
    }
}