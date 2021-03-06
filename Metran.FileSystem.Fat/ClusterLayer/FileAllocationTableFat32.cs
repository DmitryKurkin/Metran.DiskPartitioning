﻿using System;
using System.IO;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Implements a FAT32 file allocation table over an array of signed integers
    /// </summary>
    public class FileAllocationTableFat32 : FileAllocationTableBase
    {
        private const int EocMark = 0x0FFFFFFF;
        private const int BadClusterMark = 0x0FFFFFF7;

        private const int ClusterValueBits = 0x0FFFFFFF;
        private const int InverseClusterValueBits = unchecked((int) 0xF0000000);

        public FileAllocationTableFat32(
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
            RawTable = new int[dataRegionClustersCount + FirstDataCluster];

            // the reserved clusters initialization
            RawTable[0] = 0x0FFFFF00 | mediaDescriptor;
            RawTable[1] = EocMark;

            FileSystemInfo = fileSystemInfo;

            // update the FS info
            fileSystemInfo.FreeClusters = (this as IFileAllocationTable).FreeClusters;
        }

        public FileAllocationTableFat32(
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
            RawTable = new int[dataRegionClustersCount + FirstDataCluster];

            // load the table
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            LoadInternal(new BinaryReader(input));

            FileSystemInfo = fileSystemInfo;
        }

        public int[] RawTable { get; }

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
            // clear unused high-order bits
            return RawTable[cluster] & ClusterValueBits;
        }

        protected override void SetClusterValue(int cluster, int value)
        {
            // we have to preserve unused high-order bits, so:

            // 1. clear old value
            RawTable[cluster] &= InverseClusterValueBits;

            // 2. set new value
            RawTable[cluster] |= value & ClusterValueBits;
        }

        protected override void LoadInternal(BinaryReader binReader)
        {
            // load the table

            // total number of bytes to read: 'table length' times of 'size of Int32'
            var numberOfBytesToRead = RawTable.Length*4;

            // read 'raw' bytes of RawTable
            var tableBytes = binReader.ReadBytes(numberOfBytesToRead);

            // byte[] to int[] in a single operation
            Buffer.BlockCopy(tableBytes, 0, RawTable, 0, numberOfBytesToRead);
        }

        protected override void SaveInternal(BinaryWriter binWriter)
        {
            // flush the full table

            // total number of bytes to write: 'table length' times of 'size of Int32'
            var numberOfBytesToWrite = RawTable.Length*4;

            var tableBytes = new byte[numberOfBytesToWrite];

            // int[] to byte[] in a single operation
            Buffer.BlockCopy(RawTable, 0, tableBytes, 0, numberOfBytesToWrite);

            // write 'raw' bytes of RawTable
            binWriter.Write(tableBytes);
        }
    }
}