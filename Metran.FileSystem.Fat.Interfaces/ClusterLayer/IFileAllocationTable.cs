using System.IO;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Represents a FAT.
    /// Creates and edits singly-linked lists of clusters each of which can be free, used, and "bad"
    /// </summary>
    public interface IFileAllocationTable
    {
        //// space calculations
        int TotalDataClusters { get; }
        int FreeClusters { get; }
        int LastUsedCluster { get; }

        //// checking whether it points to a valid cluster in the table
        void Validate(int cluster);

        //// mapping onto a data region
        int MakeZeroBased(int cluster);

        //// chain creation
        int AllocateFirstCluster();
        int AllocateNextCluster(int previousCluster);

        //// chain reading
        int TraverseCluster(int cluster);
        bool IsLastCluster(int cluster);

        //// chain truncation
        void TruncateClusterChain(int lastClusterInUse);

        //// chain deletion
        void DeallocateClusterChain(int firstClusterToDeallocate);

        //// "bad" clusters management
        void MarkBad(int cluster);

        //// serialization
        void Save(Stream output);
    }
}