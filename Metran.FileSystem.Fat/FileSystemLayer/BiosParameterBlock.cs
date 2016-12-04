using System;
using System.IO;
using System.Text;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Represents a BPB/VBR part that is common to all FAT file systems
    /// </summary>
    public class BiosParameterBlock
    {
        public const int JumpInstructionLength = 3;
        public const int MaxOemNameLength = 8;

        // ReSharper disable once InconsistentNaming
        public const string OemNameIBM = "IBM  3.3";
        // ReSharper disable once InconsistentNaming
        public const string OemNameMSDOS = "MSDOS5.0";
        // ReSharper disable once InconsistentNaming
        public const string OemNameMSWIN = "MSWIN4.1";

        public const byte MediaDescriptorFixed = 0xF8;
        public const byte MediaDescriptorRemovable = 0xF0;

        public static readonly byte[] ValidJumpInstruction = {0xEB, 0x58, 0x90};

        private readonly byte[] _jumpInstruction;

        private byte[] _oemNameBytes;

        public BiosParameterBlock()
        {
            _jumpInstruction = new byte[JumpInstructionLength];
            _oemNameBytes = new byte[MaxOemNameLength];

            MediaDescriptor = MediaDescriptorFixed;

            JumpInstruction = ValidJumpInstruction;
            OemName = OemNameMSDOS;
        }

        public BiosParameterBlock(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            _jumpInstruction = binReader.ReadBytes(JumpInstructionLength);
            _oemNameBytes = binReader.ReadBytes(MaxOemNameLength);
            BytesPerSector = binReader.ReadUInt16();
            SectorsPerCluster = binReader.ReadByte();
            ReservedSectorsCount = binReader.ReadUInt16();
            FatsCount = binReader.ReadByte();
            RootDirectoryEntriesCount = binReader.ReadUInt16();
            VolumeSectorsCountOld = binReader.ReadUInt16();
            MediaDescriptor = binReader.ReadByte();
            FatSectorsCount = binReader.ReadUInt16();
            SectorsPerTrack = binReader.ReadUInt16();
            TracksPerCylinder = binReader.ReadUInt16();
            HiddenSectorsCount = binReader.ReadUInt32();
            VolumeSectorsCount = binReader.ReadUInt32();
        }

        public byte[] JumpInstruction
        {
            get
            {
                byte[] jumpInstructionCopy = new byte[_jumpInstruction.Length];
                _jumpInstruction.CopyTo(jumpInstructionCopy, 0);

                return jumpInstructionCopy;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length != JumpInstructionLength)
                {
                    throw new ArgumentException(
                        $"The jump instruction length ({value.Length}) is invalid. The expected value is {JumpInstructionLength}",
                        nameof(value));
                }

                value.CopyTo(_jumpInstruction, 0);
            }
        }

        public string OemName
        {
            get { return GetOemName(); }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length > MaxOemNameLength)
                {
                    throw new ArgumentException(
                        $"The OEM name length ({value.Length}) is invalid. The expected value is {MaxOemNameLength}",
                        nameof(value));
                }

                SetOemName(value);
            }
        }

        public ushort BytesPerSector { get; set; }

        public byte SectorsPerCluster { get; set; }

        public ushort ReservedSectorsCount { get; set; }

        public byte FatsCount { get; set; }

        public ushort RootDirectoryEntriesCount { get; set; }

        public ushort VolumeSectorsCountOld { get; set; }

        public byte MediaDescriptor { get; set; }

        public ushort FatSectorsCount { get; set; }

        public ushort SectorsPerTrack { get; set; }

        public ushort TracksPerCylinder { get; set; }

        public uint HiddenSectorsCount { get; set; }

        public uint VolumeSectorsCount { get; set; }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(_jumpInstruction);
            binWriter.Write(_oemNameBytes);
            binWriter.Write(BytesPerSector);
            binWriter.Write(SectorsPerCluster);
            binWriter.Write(ReservedSectorsCount);
            binWriter.Write(FatsCount);
            binWriter.Write(RootDirectoryEntriesCount);
            binWriter.Write(VolumeSectorsCountOld);
            binWriter.Write(MediaDescriptor);
            binWriter.Write(FatSectorsCount);
            binWriter.Write(SectorsPerTrack);
            binWriter.Write(TracksPerCylinder);
            binWriter.Write(HiddenSectorsCount);
            binWriter.Write(VolumeSectorsCount);
        }

        private string GetOemName()
        {
            var oemName = Encoding.ASCII.GetString(_oemNameBytes);

            oemName = oemName.TrimEnd(' ');

            return oemName;
        }

        private void SetOemName(string value)
        {
            var padded = value.PadRight(MaxOemNameLength);

            _oemNameBytes = Encoding.ASCII.GetBytes(padded);
        }
    }
}