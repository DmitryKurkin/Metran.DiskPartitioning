using System;
using System.IO;
using System.Text;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Represents a BPB/VBR part that is specific to the FAT32 file system
    /// </summary>
    public class ExtendedBiosParameterBlockFat32
    {
        public const byte PhysicalDriveNumberHardDisk = 0x80;

        public const ushort ValidFileSystemVersion = 0;
        public const uint ValidRootDirectoryCluster = 2;
        public const ushort ValidFileSystemInfoSector = 1;
        public const ushort ValidBackupSector = 6;
        public const byte ValidExtendedBootSectorSignature = 0x29;
        public const ushort ValidBootSectorSignature = 0xAA55;

        public const int MaxVolumeLabelLength = 11;
        public const int MaxFileSystemTypeLength = 8;
        public const int ReservedBytesLength = 12;
        public const int BootCodeLength = 420;

        public const string VolumeLabelDefault = "NO NAME";

        public const string FileSystemTypeFat32 = "FAT32";

        private readonly byte[] _bootCode;

        private readonly byte[] _reservedBytes;

        private byte[] _volumeLabelBytes;

        private byte[] _fileSystemTypeBytes;

        public ExtendedBiosParameterBlockFat32()
        {
            FileSystemVersion = ValidFileSystemVersion;
            RootDirectoryFirstCluster = ValidRootDirectoryCluster;
            FileSystemInfoSector = ValidFileSystemInfoSector;
            BackupSector = ValidBackupSector;
            ExtendedBootSectorSignature = ValidExtendedBootSectorSignature;
            BootSectorSignature = ValidBootSectorSignature;

            _reservedBytes = new byte[ReservedBytesLength];
            _volumeLabelBytes = new byte[MaxVolumeLabelLength];
            _fileSystemTypeBytes = new byte[MaxFileSystemTypeLength];
            _bootCode = new byte[BootCodeLength];

            PhysicalDriveNumber = PhysicalDriveNumberHardDisk;

            VolumeLabel = VolumeLabelDefault;
            FileSystemType = FileSystemTypeFat32;
        }

        public ExtendedBiosParameterBlockFat32(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            FatSectorsCount = binReader.ReadUInt32();
            ExtendedFlags = binReader.ReadUInt16();
            FileSystemVersion = binReader.ReadUInt16();
            RootDirectoryFirstCluster = binReader.ReadUInt32();
            FileSystemInfoSector = binReader.ReadUInt16();
            BackupSector = binReader.ReadUInt16();
            _reservedBytes = binReader.ReadBytes(ReservedBytesLength);
            PhysicalDriveNumber = binReader.ReadByte();
            Reserved = binReader.ReadByte();
            ExtendedBootSectorSignature = binReader.ReadByte();
            VolumeSerialNumber = binReader.ReadUInt32();
            _volumeLabelBytes = binReader.ReadBytes(MaxVolumeLabelLength);
            _fileSystemTypeBytes = binReader.ReadBytes(MaxFileSystemTypeLength);
            _bootCode = binReader.ReadBytes(BootCodeLength);
            BootSectorSignature = binReader.ReadUInt16();
        }

        public uint FatSectorsCount { get; set; }

        public ushort ExtendedFlags { get; set; }

        public ushort FileSystemVersion { get; set; }

        public uint RootDirectoryFirstCluster { get; set; }

        public ushort FileSystemInfoSector { get; set; }

        public ushort BackupSector { get; set; }

        public byte[] ReservedBytes
        {
            get
            {
                var reservedBytesCopy = new byte[_reservedBytes.Length];
                _reservedBytes.CopyTo(reservedBytesCopy, 0);

                return reservedBytesCopy;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length != ReservedBytesLength)
                {
                    throw new ArgumentException(
                        $"The reserved bytes length ({value.Length}) is invalid. The expected value is {ReservedBytesLength}",
                        nameof(value));
                }

                value.CopyTo(_reservedBytes, 0);
            }
        }

        public byte PhysicalDriveNumber { get; set; }

        public byte Reserved { get; set; }

        public byte ExtendedBootSectorSignature { get; set; }

        public uint VolumeSerialNumber { get; set; }

        public string VolumeLabel
        {
            get { return GetVolumeLabel(); }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length > MaxVolumeLabelLength)
                {
                    throw new ArgumentException(
                        $"The volume label ({value}) has an invalid length. The length must be less than {MaxVolumeLabelLength}",
                        nameof(value));
                }

                SetVolumeLabel(value);
            }
        }

        public string FileSystemType
        {
            get { return GetFileSystemType(); }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length > MaxFileSystemTypeLength)
                {
                    throw new ArgumentException(
                        $"The file system type ({value}) has an invalid length. The length must be less than {MaxFileSystemTypeLength}",
                        nameof(value));
                }

                SetFileSystemType(value);
            }
        }

        public byte[] BootCode
        {
            get
            {
                var bootCodeCopy = new byte[_bootCode.Length];
                _bootCode.CopyTo(bootCodeCopy, 0);

                return bootCodeCopy;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length != BootCodeLength)
                {
                    throw new ArgumentException(
                        $"The boot code length ({value.Length}) is invalid. The expected value is {BootCodeLength}",
                        nameof(value));
                }

                value.CopyTo(_bootCode, 0);
            }
        }

        public ushort BootSectorSignature { get; set; }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(FatSectorsCount);
            binWriter.Write(ExtendedFlags);
            binWriter.Write(FileSystemVersion);
            binWriter.Write(RootDirectoryFirstCluster);
            binWriter.Write(FileSystemInfoSector);
            binWriter.Write(BackupSector);
            binWriter.Write(_reservedBytes);
            binWriter.Write(PhysicalDriveNumber);
            binWriter.Write(Reserved);
            binWriter.Write(ExtendedBootSectorSignature);
            binWriter.Write(VolumeSerialNumber);
            binWriter.Write(_volumeLabelBytes);
            binWriter.Write(_fileSystemTypeBytes);
            binWriter.Write(_bootCode);
            binWriter.Write(BootSectorSignature);
        }

        private string GetVolumeLabel()
        {
            var volumeLabel = Encoding.ASCII.GetString(_volumeLabelBytes);

            volumeLabel = volumeLabel.TrimEnd(' ');

            return volumeLabel;
        }

        private void SetVolumeLabel(string value)
        {
            var padded = value.PadRight(MaxVolumeLabelLength);

            _volumeLabelBytes = Encoding.ASCII.GetBytes(padded);
        }

        private string GetFileSystemType()
        {
            var fileSystemType = Encoding.ASCII.GetString(_fileSystemTypeBytes);

            fileSystemType = fileSystemType.TrimEnd(' ');

            return fileSystemType;
        }

        private void SetFileSystemType(string value)
        {
            var padded = value.PadRight(MaxFileSystemTypeLength);

            _fileSystemTypeBytes = Encoding.ASCII.GetBytes(padded);
        }
    }
}