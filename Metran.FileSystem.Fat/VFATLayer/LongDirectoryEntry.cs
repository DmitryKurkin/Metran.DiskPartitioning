using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Represents a FAT 32-byte long directory entry
    /// </summary>
    public class LongDirectoryEntry
    {
        public const int MaxNameLength = 13;

        private const byte LastLongEntryMask = 0x40;
        private const byte OrderBits = 0x3F;

        private const int NameFirstPartLength = 10;
        private const int NameSecondPartLength = 12;
        private const int NameThirdPartLength = 4;

        private readonly byte[] _nameFirstPart;

        private readonly DirectoryEntryAttributes _attributes;

        private readonly byte _reserved;

        private readonly byte[] _nameSecondPart;

        private readonly ushort _firstClusterLowWord;

        private readonly byte[] _nameThirdPart;

        private byte _order;

        public LongDirectoryEntry()
        {
            _nameFirstPart = new byte[NameFirstPartLength];
            _nameSecondPart = new byte[NameSecondPartLength];
            _nameThirdPart = new byte[NameThirdPartLength];

            _attributes = DirectoryEntryAttributes.LongName;
        }

        public LongDirectoryEntry(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            _order = binReader.ReadByte();
            _nameFirstPart = binReader.ReadBytes(NameFirstPartLength);
            _attributes = (DirectoryEntryAttributes)binReader.ReadByte();
            _reserved = binReader.ReadByte();
            Checksum = binReader.ReadByte();
            _nameSecondPart = binReader.ReadBytes(NameSecondPartLength);
            _firstClusterLowWord = binReader.ReadUInt16();
            _nameThirdPart = binReader.ReadBytes(NameThirdPartLength);
        }

        public bool IsLast
        {
            get { return GetIsLast(); }
            set { SetIsLast(value); }
        }

        public byte Order
        {
            get { return GetOrder(); }
            set
            {
                if (value >= LastLongEntryMask)
                {
                    throw new ArgumentException(
                        $"The specified order value is too high: {value}",
                        nameof(value));
                }

                SetOrder(value);
            }
        }

        public string Name
        {
            get { return GetName(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The name must not be empty", nameof(value));
                }

                if (value.Length > MaxNameLength)
                {
                    throw new ArgumentException(
                        $"The name length ({value.Length}) is more than the maximum allowable value ({MaxNameLength})",
                        nameof(value));
                }

                SetName(value);
            }
        }

        public byte Checksum { get; set; }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(_order);
            binWriter.Write(_nameFirstPart);
            binWriter.Write((byte) _attributes);
            binWriter.Write(_reserved);
            binWriter.Write(Checksum);
            binWriter.Write(_nameSecondPart);
            binWriter.Write(_firstClusterLowWord);
            binWriter.Write(_nameThirdPart);
        }

        private bool GetIsLast()
        {
            return (_order & LastLongEntryMask) != 0;
        }

        private void SetIsLast(bool value)
        {
            if (value)
            {
                _order |= LastLongEntryMask;
            }
            else
            {
                _order &= unchecked((byte)~LastLongEntryMask);
            }
        }

        private byte GetOrder()
        {
            return (byte)(_order & OrderBits);
        }

        private void SetOrder(byte value)
        {
            _order = (byte)(value & OrderBits);
        }

        private string GetName()
        {
            var nameByteList = new List<byte>();
            nameByteList.AddRange(_nameFirstPart);
            nameByteList.AddRange(_nameSecondPart);
            nameByteList.AddRange(_nameThirdPart);

            var name = Encoding.Unicode.GetString(nameByteList.ToArray());

            name = name.TrimEnd('\0', '\xFFFF');

            return name;
        }

        private void SetName(string value)
        {
            var name = value;

            if (name.Length < MaxNameLength)
            {
                name = string.Concat(name, "\0");
            }

            name = name.PadRight(MaxNameLength, '\xFFFF');

            var nameByteList = new List<byte>(Encoding.Unicode.GetBytes(name));
            nameByteList.CopyTo(0, _nameFirstPart, 0, _nameFirstPart.Length);
            nameByteList.CopyTo(_nameFirstPart.Length, _nameSecondPart, 0, _nameSecondPart.Length);
            nameByteList.CopyTo(_nameFirstPart.Length + _nameSecondPart.Length, _nameThirdPart, 0, _nameThirdPart.Length);
        }
    }
}