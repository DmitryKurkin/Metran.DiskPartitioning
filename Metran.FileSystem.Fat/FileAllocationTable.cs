using System;
using System.IO;

namespace Metran.FileSystem.Fat
{
    public abstract class FileAllocationTable
    {
        protected int dataRegionClustersNumber;
        protected byte mediaDescriptor;

        protected FileAllocationTable( int dataRegionClustersNumber, byte mediaDescriptor )
        {
            if ( dataRegionClustersNumber < 1 )
            {
                throw new ArgumentException(
                    string.Format( "The number of data clusters must be greater than zero. The specified value is {0}", dataRegionClustersNumber ),
                    "dataRegionClustersNumber" );
            }

            this.dataRegionClustersNumber = dataRegionClustersNumber;
            this.mediaDescriptor = mediaDescriptor;
        }

        public abstract int AllocateCluster();
        public abstract int AllocateCluster( int previousCluster );

        public abstract int TraverseClusterChain( int currentCluster );

        public abstract void FreeClusterChain( int firstCluster );

        public abstract void TruncateClusterChain( int lastClusterInUse );

        public abstract void MarkBad( int cluster );

        public abstract void Flush( Stream output );
    }
}