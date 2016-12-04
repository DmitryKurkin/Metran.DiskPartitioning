using System;
using System.IO;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Provides the base class for a File Allocation Table
    /// </summary>
    /// <remarks>The derivatives must keep in mind that they must read the current EOC value from cluster #1 upon loading</remarks>
    public abstract class FileAllocationTableBase : IFileAllocationTable
    {
        protected const int FreeClusterMark = 0x00000000;

        protected const int ClusterNotFound = -1;

        protected const int FirstDataCluster = 2;

        protected IFileSystemInformation FileSystemInfo;

        // a count of clusters in the allowable range
        int IFileAllocationTable.TotalDataClusters => GetTableLength() - FirstDataCluster;

        int IFileAllocationTable.FreeClusters
        {
            get
            {
                var freeClusters = FindFreeClustersCount();

                return freeClusters;
            }
        }

        int IFileAllocationTable.LastUsedCluster
        {
            get
            {
                var lastUsedCluster = FindLastUsedCluster();

                return lastUsedCluster;
            }
        }

        void IFileAllocationTable.Validate(int cluster)
        {
            ValidateClusterNumber(cluster, "cluster");
        }

        int IFileAllocationTable.MakeZeroBased(int cluster)
        {
            ValidateClusterNumber(cluster, "cluster");

            // exclude the first reserved clusters
            return cluster - FirstDataCluster;
        }

        int IFileAllocationTable.AllocateFirstCluster()
        {
            // find a free cluster
            var firstFreeCluster = FindFirstFreeCluster();

            if (firstFreeCluster == ClusterNotFound)
            {
                throw new FileAllocationTableFullException("Failed to allocate a cluster. The table is full");
            }

            // make it "used" and update the FS info
            SetClusterValue(firstFreeCluster, GetEocMark());
            UpdateFileSystemInfoOnAllocation(firstFreeCluster);

            return firstFreeCluster;
        }

        int IFileAllocationTable.AllocateNextCluster(int previousCluster)
        {
            ValidateClusterNumber(previousCluster, "previousCluster");

            // find a free cluster
            var nextFreeCluster = FindFirstFreeCluster();

            if (nextFreeCluster == ClusterNotFound)
            {
                throw new FileAllocationTableFullException("Failed to allocate a cluster. The table is full");
            }

            // make it "used" and update the FS info
            SetClusterValue(nextFreeCluster, GetEocMark());
            UpdateFileSystemInfoOnAllocation(nextFreeCluster);

            // connect the two clusters into a chain (make the 1st pointing to the 2nd)
            SetClusterValue(previousCluster, nextFreeCluster);

            return nextFreeCluster;
        }

        int IFileAllocationTable.TraverseCluster(int cluster)
        {
            ValidateClusterNumber(cluster, "cluster");

            // just get the next cluster
            var nextCluster = GetClusterValue(cluster);

            return nextCluster;
        }

        bool IFileAllocationTable.IsLastCluster(int cluster)
        {
            ValidateClusterNumber(cluster, "cluster");

            var nextCluster = GetClusterValue(cluster);

            // the last cluster in a chain has EocMark value
            return nextCluster == GetEocMark();
        }

        void IFileAllocationTable.TruncateClusterChain(int lastClusterInUse)
        {
            ValidateClusterNumber(lastClusterInUse, "lastClusterInUse");

            // do nothing if this is already the last cluster
            if (!(this as IFileAllocationTable).IsLastCluster(lastClusterInUse))
            {
                // all subsequent clusters will be freed
                var firstClusterToFree = GetClusterValue(lastClusterInUse);

                // make it the last cluster in the chain
                SetClusterValue(lastClusterInUse, GetEocMark());

                // free subsequent clusters
                (this as IFileAllocationTable).DeallocateClusterChain(firstClusterToFree);
            }
        }

        void IFileAllocationTable.DeallocateClusterChain(int firstClusterToDeallocate)
        {
            ValidateClusterNumber(firstClusterToDeallocate, "firstClusterToDeallocate");

            // we'll have to traverse the chain
            var currentCluster = firstClusterToDeallocate;

            do
            {
                // read the cluster the current one points to
                var nextCluster = GetClusterValue(currentCluster);

                // make the current cluster "free" and update the FS info
                SetClusterValue(currentCluster, FreeClusterMark);
                UpdateFileSystemInfoOnFreeing();

                // go to the next cluster (the one that was read at the first step)
                currentCluster = nextCluster;
            } while (currentCluster != GetEocMark());
        }

        void IFileAllocationTable.MarkBad(int cluster)
        {
            ValidateClusterNumber(cluster, "cluster");

            // make it "bad" and update the FS info
            SetClusterValue(cluster, GetBadClusterMark());
            UpdateFileSystemInfoOnAllocation(cluster);
        }

        void IFileAllocationTable.Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            SaveInternal(binWriter);
        }

        protected abstract int GetTableLength();

        protected abstract int GetEocMark();

        protected abstract int GetBadClusterMark();

        protected abstract int GetClusterValue(int cluster);

        protected abstract void SetClusterValue(int cluster, int value);

        protected abstract void LoadInternal(BinaryReader binReader);

        protected abstract void SaveInternal(BinaryWriter binWriter);

        protected void ValidateClusterNumber(int cluster, string parameterName)
        {
            if (cluster < FirstDataCluster)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"The specified cluster number ({cluster}) is less than the first valid data cluster ({FirstDataCluster})");
            }

            if (cluster >= GetTableLength())
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"The specified cluster number ({cluster}) is larger than the table length ({GetTableLength()})");
            }
        }

        protected int FindFirstFreeCluster()
        {
            var firstFreeCluster = ClusterNotFound;

            // the first one (starting from the FirstDataCluster) that has the FreeClusterMark value...
            for (var i = FirstDataCluster; i < GetTableLength(); i++)
            {
                if (GetClusterValue(i) == FreeClusterMark)
                {
                    firstFreeCluster = i;
                    break;
                }
            }

            return firstFreeCluster;
        }

        protected int FindFreeClustersCount()
        {
            var freeClustersCount = 0;

            // collect all that have the FreeClusterMark value
            for (var i = FirstDataCluster; i < GetTableLength(); i++)
            {
                if (GetClusterValue(i) == FreeClusterMark)
                {
                    freeClustersCount++;
                }
            }

            return freeClustersCount;
        }

        protected int FindLastUsedCluster()
        {
            var lastUsedCluster = ClusterNotFound;

            // the last one (ending with the FirstDataCluster) that doesn't have the FreeClusterMark value...
            for (var i = GetTableLength() - 1; i >= FirstDataCluster; i--)
            {
                if (GetClusterValue(i) != FreeClusterMark)
                {
                    lastUsedCluster = i;
                    break;
                }
            }

            return lastUsedCluster;
        }

        private void UpdateFileSystemInfoOnAllocation(int allocatedCluster)
        {
            FileSystemInfo.FreeClusters--;
            FileSystemInfo.LastAllocatedCluster = allocatedCluster;
        }

        private void UpdateFileSystemInfoOnFreeing()
        {
            FileSystemInfo.FreeClusters++;
        }
    }
}