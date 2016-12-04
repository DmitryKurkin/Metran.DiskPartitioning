using System.IO;

namespace Metran.DiskPartitioning
{
    /// <summary>
    /// Manages a partition table on a hard disk drive
    /// </summary>
    public interface IPartitionTableManager
    {
        IPartition[] Partitions { get; }

        int MaxPartitions { get; }

        int TotalSectors { get; }

        int CreatePartition(bool isActive, byte partitionType, uint firtsSectorAddress, uint sectorsCount);

        void ChangePartition(
            int partitionIndex,
            bool isActive,
            byte partitionType,
            uint firtsSectorAddress,
            uint sectorsCount);

        void DeletePartition(int partitionIndex);

        void Save(Stream output);
    }
}