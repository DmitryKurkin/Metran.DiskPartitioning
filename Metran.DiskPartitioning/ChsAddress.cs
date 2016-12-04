using System;
using System.IO;

namespace Metran.DiskPartitioning
{
    /// <summary>
    /// Represents an address in the Cylinder-Head-Sector format
    /// </summary>
    public class ChsAddress
    {
        public const ushort MaxCylinder = 1023;

        public const byte MaxHead = 254;

        public const byte MaxSector = 63;

        public static ChsAddress MaxAddress => new ChsAddress(MaxCylinder, MaxHead, MaxSector);

        private ushort _cylinderAndSectorBits;

        public ChsAddress(byte head, ushort cylinderAndSectorBits)
        {
            Head = head;
            _cylinderAndSectorBits = cylinderAndSectorBits;
        }

        public ChsAddress()
            : this(0, 0)
        {
        }

        public ChsAddress(ushort cylinder, byte head, byte sector)
        {
            Head = head;

            Cylinder = cylinder;
            Sector = sector;
        }

        public ChsAddress(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            Head = binReader.ReadByte();
            _cylinderAndSectorBits = binReader.ReadUInt16();
        }

        public ushort Cylinder
        {
            get { return (ushort) (_cylinderAndSectorBits >> 6); }
            internal set
            {
                if (value >= 1024)
                {
                    throw new ArgumentException(
                        $"The value does not fit in 10 bits: {value}",
                        nameof(value));
                }

                _cylinderAndSectorBits &= 0x3F; // 0000'0000'0011'1111 -> clear the cylinder
                _cylinderAndSectorBits |= (ushort) (value << 6);
            }
        }

        public byte Head { get; internal set; }

        public byte Sector
        {
            get { return (byte) (_cylinderAndSectorBits & 0x3F); }
            internal set
            {
                if (value >= 64)
                {
                    throw new ArgumentException(
                        $"The value does not fit in 6 bits: {value}",
                        nameof(value));
                }

                _cylinderAndSectorBits &= 0xFFC0; // 1111'1111'1100'0000 -> clear the sector
                _cylinderAndSectorBits |= (ushort) (value & 0x3F);
            }
        }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(Head);
            binWriter.Write(_cylinderAndSectorBits);
        }
    }
}