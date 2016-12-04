using System;
using System.IO;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Parses directory entries from an array of bytes
    /// </summary>
    /// <remarks>The implementation does not take into account the possible first byte set to 0x05 that requires special handling</remarks>
    public class DirectoryEntryParser
    {
        public const int DirectoryEntryLength = 32;

        private const int FirstNameByteOffset = 0;
        private const int AttributesOffset = 11;

        private const byte LastEntryMark = 0x00;
        private const byte FreeEntryMark = 0xE5;

        private const DirectoryEntryAttributes LongDirectoryEntryMask =
            DirectoryEntryAttributes.ReadOnly |
            DirectoryEntryAttributes.Hidden |
            DirectoryEntryAttributes.System |
            DirectoryEntryAttributes.VolumeLabel |
            DirectoryEntryAttributes.Directory |
            DirectoryEntryAttributes.Archive;

        public static DirectoryEntryParseResult Parse(byte[] entryBytes, out object parsedEntry)
        {
            if (entryBytes == null) throw new ArgumentNullException(nameof(entryBytes));

            if (entryBytes.Length != DirectoryEntryLength)
            {
                throw new ArgumentException(
                    $"The entry length ({entryBytes.Length}) is invalid. The expected value is {DirectoryEntryLength}",
                    nameof(entryBytes));
            }

            // an invalid entry by default
            parsedEntry = null;
            var result = DirectoryEntryParseResult.InvalidEntry;

            if (entryBytes[FirstNameByteOffset] == LastEntryMark)
            {
                result = DirectoryEntryParseResult.LastEntry;
            }
            else if (entryBytes[FirstNameByteOffset] == FreeEntryMark)
            {
                result = DirectoryEntryParseResult.FreeEntry;
            }
            else
            {
                var atts = (DirectoryEntryAttributes) entryBytes[AttributesOffset];

                // either a long entry...
                if ((atts & LongDirectoryEntryMask) == DirectoryEntryAttributes.LongName)
                {
                    parsedEntry = new LongDirectoryEntry(new MemoryStream(entryBytes));
                    result = DirectoryEntryParseResult.LongEntry;
                }
                else
                {
                    // ...or a short one

                    // use it for convenience
                    const DirectoryEntryAttributes dirOrVolumeLabel = DirectoryEntryAttributes.Directory |
                                                                      DirectoryEntryAttributes.VolumeLabel;

                    if ((atts & dirOrVolumeLabel) == 0x00)
                    {
                        // found a file
                        using (var input = new MemoryStream(entryBytes))
                        {
                            parsedEntry = new ShortDirectoryEntry(input);
                            result = DirectoryEntryParseResult.ShortEntry;
                        }
                    }
                    else if ((atts & dirOrVolumeLabel) == DirectoryEntryAttributes.Directory)
                    {
                        // found a directory
                        using (var input = new MemoryStream(entryBytes))
                        {
                            parsedEntry = new ShortDirectoryEntry(input);
                            result = DirectoryEntryParseResult.ShortEntry;
                        }
                    }
                    else if ((atts & dirOrVolumeLabel) == DirectoryEntryAttributes.VolumeLabel)
                    {
                        // found a volume label
                        using (var input = new MemoryStream(entryBytes))
                        {
                            parsedEntry = new VolumeLabelEntry(input);
                            result = DirectoryEntryParseResult.VolumeLabelEntry;
                        }
                    }
                }
            }

            return result;
        }
    }
}