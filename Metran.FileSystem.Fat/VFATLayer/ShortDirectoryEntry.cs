using System;
using System.IO;
using System.Text;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Represents a FAT 32-byte short directory entry
    /// </summary>
    public class ShortDirectoryEntry
    {
        public const int MaxNameMainPartLength = 8;
        public const int MaxNameExtensionLength = 3;

        private readonly byte _reserved;

        private byte[] _nameMainPartBytes;

        private byte[] _nameExtensionBytes;

        private byte _creationTimeMillisecondsTenths;

        private ushort _creationTime;

        private ushort _creationDate;

        private ushort _lastAccessDate;

        private ushort _firstClusterHighWord;

        private ushort _lastWriteTime;

        private ushort _lastWriteDate;

        private ushort _firstClusterLowWord;

        public ShortDirectoryEntry()
        {
            _nameMainPartBytes = new byte[MaxNameMainPartLength];
            _nameExtensionBytes = new byte[MaxNameExtensionLength];
        }

        public ShortDirectoryEntry(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            _nameMainPartBytes = binReader.ReadBytes(MaxNameMainPartLength);
            _nameExtensionBytes = binReader.ReadBytes(MaxNameExtensionLength);
            EntryAttributes = (DirectoryEntryAttributes) binReader.ReadByte();
            _reserved = binReader.ReadByte();
            _creationTimeMillisecondsTenths = binReader.ReadByte();
            _creationTime = binReader.ReadUInt16();
            _creationDate = binReader.ReadUInt16();
            _lastAccessDate = binReader.ReadUInt16();
            _firstClusterHighWord = binReader.ReadUInt16();
            _lastWriteTime = binReader.ReadUInt16();
            _lastWriteDate = binReader.ReadUInt16();
            _firstClusterLowWord = binReader.ReadUInt16();
            Size = binReader.ReadUInt32();
        }

        public string NameMainPart
        {
            get { return GetNameMainPart(); }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length > MaxNameMainPartLength)
                {
                    throw new ArgumentException(
                        $"The name main part length ({value.Length}) is more than the maximum allowable value ({MaxNameMainPartLength})",
                        nameof(value));
                }

                SetNameMainPart(value);
            }
        }

        public string NameExtension
        {
            get { return GetNameExtension(); }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length > MaxNameExtensionLength)
                {
                    throw new ArgumentException(
                        $"The name extension length ({value.Length}) is more than the maximum allowable value ({MaxNameExtensionLength})",
                        nameof(value));
                }

                SetNameExtension(value);
            }
        }

        public DirectoryEntryAttributes EntryAttributes { get; set; }

        public DateTime CreationDate
        {
            get { return FatDateTime.Pack(_creationDate, _creationTime, _creationTimeMillisecondsTenths); }
            set
            {
                FatDateTime.Unpack(value, out _creationDate, out _creationTime, out _creationTimeMillisecondsTenths);
            }
        }

        public DateTime LastAccessDate
        {
            get { return FatDateTime.Pack(_lastAccessDate); }
            set { FatDateTime.Unpack(value, out _lastAccessDate); }
        }

        public DateTime LastWriteDate
        {
            get { return FatDateTime.Pack(_lastWriteDate, _lastWriteTime); }
            set { FatDateTime.Unpack(value, out _lastWriteDate, out _lastWriteTime); }
        }

        public int FirstCluster
        {
            get { return Utils.PackToInt32(_firstClusterHighWord, _firstClusterLowWord); }
            set { Utils.Unpack(value, out _firstClusterHighWord, out _firstClusterLowWord); }
        }

        public uint Size { get; set; }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(_nameMainPartBytes);
            binWriter.Write(_nameExtensionBytes);
            binWriter.Write((byte) EntryAttributes);
            binWriter.Write(_reserved);
            binWriter.Write(_creationTimeMillisecondsTenths);
            binWriter.Write(_creationTime);
            binWriter.Write(_creationDate);
            binWriter.Write(_lastAccessDate);
            binWriter.Write(_firstClusterHighWord);
            binWriter.Write(_lastWriteTime);
            binWriter.Write(_lastWriteDate);
            binWriter.Write(_firstClusterLowWord);
            binWriter.Write(Size);
        }

        private string GetNameMainPart()
        {
            var nameMainPart = Encoding.ASCII.GetString(_nameMainPartBytes);

            return nameMainPart;
        }

        private void SetNameMainPart(string nameMainPart)
        {
            // don't forget to pad it with spaces...

            var spacePaddedNameMainPart = nameMainPart.PadRight(MaxNameMainPartLength);

            _nameMainPartBytes = Encoding.ASCII.GetBytes(spacePaddedNameMainPart);
        }

        private string GetNameExtension()
        {
            var extension = Encoding.ASCII.GetString(_nameExtensionBytes);

            return extension;
        }

        private void SetNameExtension(string nameExtension)
        {
            // don't forget to pad it with spaces...

            var spacePaddedNameExtension = nameExtension.PadRight(MaxNameExtensionLength);

            _nameExtensionBytes = Encoding.ASCII.GetBytes(spacePaddedNameExtension);
        }
    }
}