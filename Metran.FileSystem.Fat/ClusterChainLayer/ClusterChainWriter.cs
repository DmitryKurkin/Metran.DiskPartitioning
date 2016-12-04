using Metran.FileSystem.Fat.ClusterLayer;
using System;

namespace Metran.FileSystem.Fat.ClusterChainLayer
{
    /// <summary>
    /// Writes data to a chain of clusters using a FAT and Data Region. Allocates one cluster ahead. Truncates the last cluster if there is more than two clusters in the chain upon closing
    /// </summary>
    public class ClusterChainWriter : IClusterChainWriter
    {
        protected IFileAllocationTable Fat;

        protected IDataRegion DataRegion;

        protected int UnderlyingFirstCluster;

        protected int PreviousCluster;

        protected int CurrentCluster;

        protected bool ResetAllocatedClustersData;

        protected bool IsClosed;

        public ClusterChainWriter(IFileAllocationTable fat, IDataRegion dataRegion, bool resetAllocatedClustersData)
        {
            if (fat == null) throw new ArgumentNullException(nameof(fat));
            if (dataRegion == null) throw new ArgumentNullException(nameof(dataRegion));

            if (dataRegion.ClustersCount != fat.TotalDataClusters)
            {
                throw new ArgumentException(
                    $"The data region must have the same length as the FAT does excluding reserved clusters ({fat.TotalDataClusters}). The current data region length is {dataRegion.ClustersCount}",
                    nameof(dataRegion));
            }

            Fat = fat;
            DataRegion = dataRegion;
            ResetAllocatedClustersData = resetAllocatedClustersData;

            // allocate the very first cluster of the chain here in the constructor
            UnderlyingFirstCluster = fat.AllocateFirstCluster();

            // use the first cluster in the next call to the WriteNextCluster (reset it if needed)
            PreviousCluster = Utils.ClusterNotAllocated; // does not exist for now
            CurrentCluster = UnderlyingFirstCluster;
            ResetCurrentClusterDataIfNeeds();
        }

        protected ClusterChainWriter()
        {
        }

        int IClusterChainWriter.BytesPerCluster => DataRegion.SectorsPerCluster*DataRegion.BytesPerSector;

        int IClusterChainWriter.FirstCluster => UnderlyingFirstCluster;

        void IClusterChainWriter.WriteNextCluster(byte[] clusterData)
        {
            // make sure we did not close
            AssertNotClosed();

            // use the current cluster and write the data to it
            DataRegion.WriteCluster(Fat.MakeZeroBased(CurrentCluster), clusterData);

            // save the current cluster
            PreviousCluster = CurrentCluster;

            // allocate the next one
            CurrentCluster = SelectNextCluster();

            // reset the current cluster if it needs to
            ResetCurrentClusterDataIfNeeds();
        }

        void IClusterChainWriter.Close()
        {
            AssertNotClosed();

            // the implementation uses one cluster ahead.
            // now we need to truncate that last unused cluster (that nobody needs)

            // if there are 2+ clusters...
            if (PreviousCluster != Utils.ClusterNotAllocated)
            {
                // the previous cluster was the actual last cluster in the chain
                Fat.TruncateClusterChain(PreviousCluster);

                // reset it 
                CurrentCluster = Utils.ClusterNotAllocated;
            }

            // we're done
            IsClosed = true;
        }

        protected void AssertNotClosed()
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("The writer has been closed");
            }
        }

        protected void ResetCurrentClusterDataIfNeeds()
        {
            // were we asked to do this?
            if (ResetAllocatedClustersData)
            {
                // zero data to be put to the current cluster
                var clusterData = new byte[(this as IClusterChainWriter).BytesPerCluster];

                // initialize the current cluster with zeroes
                DataRegion.WriteCluster(Fat.MakeZeroBased(CurrentCluster), clusterData);
            }
        }

        protected virtual int SelectNextCluster()
        {
            var nextCluster = Fat.AllocateNextCluster(CurrentCluster);

            return nextCluster;
        }
    }
}