using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Represents a FAT32 FSInfo sector
    /// </summary>
    public class FileSystemInformation : IFileSystemInformation
    {
        public const uint LeadingSignature = 0x41615252;
        public const uint StructureSignature = 0x61417272;
        public const uint TrailingSignature = 0xAA550000;

        public const int Reserved1BytesCount = 480;
        public const int Reserved2BytesCount = 12;

        private int _freeClusters;

        private int _lastAllocatedCluster;

        public FileSystemInformation()
        {
            _freeClusters = -1;
            _lastAllocatedCluster = -1;
        }

        [SuppressMessage("ReSharper", "UnusedVariable")]
        public FileSystemInformation(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            var leadSignature = binReader.ReadUInt32();
            var reserved1 = binReader.ReadBytes(Reserved1BytesCount);
            var structSignature = binReader.ReadUInt32();

            _freeClusters = binReader.ReadInt32();
            _lastAllocatedCluster = binReader.ReadInt32();

            var reserved2 = binReader.ReadBytes(Reserved2BytesCount);
            var trailSignature = binReader.ReadUInt32();
        }

        int IFileSystemInformation.FreeClusters
        {
            get { return _freeClusters; }
            set { _freeClusters = value; }
        }

        int IFileSystemInformation.LastAllocatedCluster
        {
            get { return _lastAllocatedCluster; }
            set { _lastAllocatedCluster = value; }
        }

        void IFileSystemInformation.Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(LeadingSignature);
            binWriter.Write(new byte[Reserved1BytesCount]);
            binWriter.Write(StructureSignature);
            binWriter.Write(_freeClusters);
            binWriter.Write(_lastAllocatedCluster);
            binWriter.Write(new byte[Reserved2BytesCount]);
            binWriter.Write(TrailingSignature);
        }
    }
}