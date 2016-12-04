namespace Metran.DiskPartitioning
{
    /// <summary>
    /// Represents a logical storage unit on a hard disk drive
    /// </summary>
    public interface IPartition
    {
        bool IsActive { get; }

        byte PartitionType { get; }

        uint FirstSectorAddress { get; }

        uint SectorsCount { get; }
    }
}