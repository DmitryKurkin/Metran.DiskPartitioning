using Metran.FileSystem.Fat.ClusterLayer;
using System;

namespace Metran.FileSystem.Fat.ClusterChainLayer
{
    /// <summary>
    /// Overwrites data of a chain of clusters using a FAT and Data Region. Truncates the rest of the chain (if any) upon closing.
    /// Starts to allocate one cluster ahead when all existing clusters have been overwritten. Truncates the last cluster if there is more than two clusters in the chain upon closing
    /// </summary>
    public class ClusterChainOverwriter : ClusterChainWriter
    {
        protected override int SelectNextCluster()
        {
            int nextCluster;

            // is the current cluster the last one in the chain?
            if (Fat.IsLastCluster(CurrentCluster))
            {
                // yes, it is the last one

                // the next cluster should be allocated
                nextCluster = Fat.AllocateNextCluster(CurrentCluster);
            }
            else
            {
                // no, there are more clusters in the chain

                // the next cluster already exists
                nextCluster = Fat.TraverseCluster(CurrentCluster);
            }

            return nextCluster;
        }

        public ClusterChainOverwriter(
            IFileAllocationTable fat,
            IDataRegion dataRegion,
            bool resetAllocatedClustersData,
            int firstCluster)
        {
            if (fat == null) throw new ArgumentNullException(nameof(fat));
            if (dataRegion == null) throw new ArgumentNullException(nameof(dataRegion));

            if (dataRegion.ClustersCount != fat.TotalDataClusters)
            {
                throw new ArgumentException(
                    $"The data region must have the same length as the FAT does excluding reserved clusters ({fat.TotalDataClusters}). The current data region length is {dataRegion.ClustersCount}",
                    nameof(dataRegion));
            }

            fat.Validate(firstCluster);

            Fat = fat;
            DataRegion = dataRegion;
            ResetAllocatedClustersData = resetAllocatedClustersData;
            UnderlyingFirstCluster = firstCluster;

            // the first cluster is already allocated,
            // so we can use it as is in the next call to the WriteNextCluster
            PreviousCluster = Utils.ClusterNotAllocated; // does not exist for now
            CurrentCluster = firstCluster;

            // reset the current cluster if needed
            ResetCurrentClusterDataIfNeeds();
        }
    }
}