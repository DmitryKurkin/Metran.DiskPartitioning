using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Implements the logical association between a short directory entry and a number of long directory entries.
    /// Changes the number of long directory entries as necessary
    /// </summary>
    /// <remarks>
    /// If the specified list of long entries is empty, it thinks the whole entry is ok and uses just the short entry
    /// </remarks>
    public class CompositeDirectoryEntry : IDirectoryEntry
    {
        public const int MaxLongNameLength = 255;

        private readonly ShortDirectoryEntry _shortEntry;

        private readonly List<LongDirectoryEntry> _longEntries;

        public CompositeDirectoryEntry()
        {
            _shortEntry = new ShortDirectoryEntry();
            _longEntries = new List<LongDirectoryEntry>();
        }

        internal CompositeDirectoryEntry(
            ShortDirectoryEntry shortEntry,
            IEnumerable<LongDirectoryEntry> longEntries)
        {
            if (shortEntry == null) throw new ArgumentNullException(nameof(shortEntry));
            if (longEntries == null) throw new ArgumentNullException(nameof(longEntries));

            _shortEntry = shortEntry;
            _longEntries = new List<LongDirectoryEntry>(longEntries);
        }

        public string Name => GetName();

        public DirectoryEntryAttributes EntryAttributes
        {
            get { return _shortEntry.EntryAttributes; }
            set { _shortEntry.EntryAttributes = value; }
        }

        public DateTime CreationDate
        {
            get { return _shortEntry.CreationDate; }
            set { _shortEntry.CreationDate = value; }
        }

        public DateTime LastAccessDate
        {
            get { return _shortEntry.LastAccessDate; }
            set { _shortEntry.LastAccessDate = value; }
        }

        public DateTime LastWriteDate
        {
            get { return _shortEntry.LastWriteDate; }
            set { _shortEntry.LastWriteDate = value; }
        }

        public int FirstCluster
        {
            get { return _shortEntry.FirstCluster; }
            set { _shortEntry.FirstCluster = value; }
        }

        public uint Size
        {
            get { return _shortEntry.Size; }
            set { _shortEntry.Size = value; }
        }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            // the long entries go first in the reverse order
            for (var i = _longEntries.Count - 1; i >= 0; i--)
            {
                _longEntries[i].Save(output);
            }

            // the short entry goes last
            _shortEntry.Save(output);
        }

        // one short entry and a number of long ones (in bytes)
        internal int TotalSize => DirectoryEntryParser.DirectoryEntryLength*(1 + _longEntries.Count);

        internal string ShortName => GetShortName(true);

        internal void LoadLongName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The long name must not be empty", nameof(value));
            }

            if (value.Length > MaxLongNameLength)
            {
                throw new ArgumentException(
                    $"The long name length ({value.Length}) is more than the maximum allowable value ({MaxLongNameLength})",
                    nameof(value));
            }

            // remove any existing ones
            _longEntries.Clear();

            // we will be decreasing this number of chars
            var totalCharsToCopy = value.Length;

            // the number of the current long name entry
            var entryNumber = 0;

            do
            {
                // calculate the current number of chars to copy:
                // either a whole LongDirectoryEntry.MaxNameLength or a piece of it
                var currentNamePartLength =
                    totalCharsToCopy > LongDirectoryEntry.MaxNameLength
                        ? LongDirectoryEntry.MaxNameLength
                        : totalCharsToCopy;

                // decrease the total number by the just calculated one
                totalCharsToCopy -= currentNamePartLength;

                // create a long name entry and set its name (as a subset of the whole name passed into the method)
                var lfn = new LongDirectoryEntry
                {
                    Name = value.Substring(
                        entryNumber*LongDirectoryEntry.MaxNameLength,
                        currentNamePartLength)
                };

                // add it into the list
                _longEntries.Add(lfn);

                // go to the next entry
                entryNumber++;
            } while (totalCharsToCopy > 0);
        }

        internal void LoadShortName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The short name must not be empty", nameof(value));
            }

            // analyze a possible extension
            var portions = value.Split('.');
            if (portions.Length == 1)
            {
                // no periods

                if (portions[0].Length > ShortDirectoryEntry.MaxNameMainPartLength)
                {
                    throw new ArgumentException(
                        $"The main part length ({portions[0].Length}) is more than the maximum allowable value ({ShortDirectoryEntry.MaxNameMainPartLength})",
                        nameof(value));
                }

                // just the main part, no extension
                _shortEntry.NameMainPart = portions[0];
                _shortEntry.NameExtension = string.Empty;
            }
            else if (portions.Length == 2)
            {
                // one period

                if (portions[0].Length > ShortDirectoryEntry.MaxNameMainPartLength)
                {
                    throw new ArgumentException(
                        $"The main part length ({portions[0].Length}) is more than the maximum allowable value ({ShortDirectoryEntry.MaxNameMainPartLength})",
                        nameof(value));
                }

                if (portions[1].Length > ShortDirectoryEntry.MaxNameExtensionLength)
                {
                    throw new ArgumentException(
                        $"The extension length ({portions[1].Length}) is more than the maximum allowable value ({ShortDirectoryEntry.MaxNameExtensionLength})",
                        nameof(value));
                }

                // both the main part and extension
                _shortEntry.NameMainPart = portions[0];
                _shortEntry.NameExtension = portions[1];
            }
            else
            {
                // more than one period

                throw new ArgumentException("The short name must not contain a period more than once", nameof(value));
            }
        }

        internal bool IsValid()
        {
            // if there are no long entries, we consider the entire entry is valid
            var result = true;

            if (_longEntries.Count > 0)
            {
                // the check strategy is as follows:
                // 1. all checksums are valid
                // 2. all orders are valid
                // 3. just the last one is marked with the flag

                var checksum = CalculateChecksum();

                for (var i = 0; i < _longEntries.Count; i++)
                {
                    // the current checksum
                    if (_longEntries[i].Checksum != checksum)
                    {
                        result = false;
                        break;
                    }

                    // the current order
                    if (_longEntries[i].Order != (byte) (i + 1))
                    {
                        result = false;
                        break;
                    }

                    // the current flag
                    if (i != _longEntries.Count - 1)
                    {
                        // this is not the last one, the flag must be reset
                        if (_longEntries[i].IsLast)
                        {
                            result = false;
                            break;
                        }
                    }
                    else
                    {
                        // this is the last one, the flag must be set
                        if (!_longEntries[i].IsLast)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        internal void Rebuild()
        {
            // if there are no long entries, we consider the entire entry is valid and does not require rebuilding

            if (_longEntries.Count > 0)
            {
                // the rebuild strategy:
                // 1. checksums
                // 2. orders
                // 3. the flag

                var checksum = CalculateChecksum();

                for (var i = 0; i < _longEntries.Count; i++)
                {
                    _longEntries[i].Checksum = checksum;
                    _longEntries[i].IsLast = false;
                    _longEntries[i].Order = (byte) (i + 1);
                }

                // for the last entry in the list
                _longEntries[_longEntries.Count - 1].IsLast = true;
            }
        }

        private string GetShortName(bool insertPeriod)
        {
            string shortName;

            // what do we have to return: a plain full name with padding spaces or an adopted one?
            if (insertPeriod)
            {
                // remove the padding spaces from the main part and extension
                var mainPart = _shortEntry.NameMainPart.TrimEnd();
                var extension = _shortEntry.NameExtension.TrimEnd();

                // has the extension gone after the removing?
                if (extension.Length != 0)
                {
                    // no, it still exists
                    shortName = $"{mainPart}.{extension}";
                }
                else
                {
                    // it has gone, use just the main part
                    shortName = mainPart;
                }
            }
            else
            {
                // just concatenate the two parts (used for a checksum calculation)
                shortName = $"{_shortEntry.NameMainPart}{_shortEntry.NameExtension}";
            }

            return shortName;
        }

        private string GetLongName()
        {
            var longName = string.Empty;

            // concatenate the parts...
            foreach (var lfn in _longEntries)
            {
                longName = $"{longName}{lfn.Name}";
            }

            return longName;
        }

        private string GetName()
        {
            // either the long name or the short one
            var name = _longEntries.Count > 0 ? GetLongName() : ShortName;

            return name;
        }

        private byte CalculateChecksum()
        {
            byte checksum = 0;

            var shortName = Encoding.GetEncoding(866).GetBytes(GetShortName(false));

            foreach (var t in shortName)
            {
                // rotate the current value and add the current char
                checksum = (byte) (Utils.RotateRight(checksum, 1) + t);
            }

            return checksum;
        }
    }
}