using System;
using System.IO;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Implements a FAT16 file allocation table over an array of unsigned shorts
    /// </summary>
    public class FileAllocationTableFat16 : FileAllocationTableBase
    {
        private const ushort EocMark = 0xFFFF;
        private const ushort BadClusterMark = 0xFFF7;

        private const int ClusterValueBits = 0x0000FFFF;

        public FileAllocationTableFat16(
            int dataRegionClustersCount,
            byte mediaDescriptor,
            IFileSystemInformation fileSystemInfo)
        {
            if (fileSystemInfo == null) throw new ArgumentNullException(nameof(fileSystemInfo));

            if (dataRegionClustersCount < 1)
            {
                throw new ArgumentException(
                    $"The number of data clusters must be greater than zero. The specified value is {dataRegionClustersCount}",
                    nameof(dataRegionClustersCount));
            }

            if (dataRegionClustersCount + FirstDataCluster >= BadClusterMark)
            {
                throw new ArgumentException(
                    $"The specified number of data clusters ({dataRegionClustersCount}) is larger than the maximum allowable value ({BadClusterMark - 1})",
                    nameof(dataRegionClustersCount));
            }

            // the number of actual data clusters plus two reserved ones
            RawTable = new ushort[dataRegionClustersCount + FirstDataCluster];

            // the reserved clusters initialization
            RawTable[0] = (ushort)(0xFF00 | mediaDescriptor);
            RawTable[1] = EocMark;

            FileSystemInfo = fileSystemInfo;

            // update the FS info
            fileSystemInfo.FreeClusters = (this as IFileAllocationTable).FreeClusters;
        }

        public FileAllocationTableFat16(
            Stream input,
            int dataRegionClustersCount,
            IFileSystemInformation fileSystemInfo)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (fileSystemInfo == null) throw new ArgumentNullException(nameof(fileSystemInfo));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            if (dataRegionClustersCount < 1)
            {
                throw new ArgumentException(
                    $"The number of data clusters must be greater than zero. The specified value is {dataRegionClustersCount}",
                    nameof(dataRegionClustersCount));
            }

            if (dataRegionClustersCount + FirstDataCluster >= BadClusterMark)
            {
                throw new ArgumentException(
                    $"The specified number of data clusters ({dataRegionClustersCount}) is larger than the maximum allowable value ({BadClusterMark - 1})",
                    nameof(dataRegionClustersCount));
            }

            // the number of actual data clusters plus two reserved ones
            RawTable = new ushort[dataRegionClustersCount + FirstDataCluster];

            // load the table
            // ReSharper disable once VirtualMemberCallInConstructor
            LoadInternal(new BinaryReader(input));

            FileSystemInfo = fileSystemInfo;
        }

        public ushort[] RawTable { get; }

        protected override int GetTableLength()
        {
            return RawTable.Length;
        }

        protected override int GetEocMark()
        {
            return EocMark;
        }

        protected override int GetBadClusterMark()
        {
            return BadClusterMark;
        }

        protected override int GetClusterValue(int cluster)
        {
            return RawTable[cluster];
        }

        protected override void SetClusterValue(int cluster, int value)
        {
            RawTable[cluster] = (ushort) (value & ClusterValueBits);
        }

        protected override void LoadInternal(BinaryReader binReader)
        {
            // load the table
            for (var i = 0; i < RawTable.Length; i++)
            {
                RawTable[i] = binReader.ReadUInt16();
            }
        }

        protected override void SaveInternal(BinaryWriter binWriter)
        {
            // flush the full table
            for (var i = 0; i < RawTable.Length; i++)
            {
                binWriter.Write(RawTable[i]);
            }
        }
    }
}