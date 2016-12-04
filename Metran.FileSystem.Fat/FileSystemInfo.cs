using System.IO;

namespace Metran.FileSystem.Fat
{
    public abstract class FileSystemInfo
    {
        private int freeClusters;
        private int lastAllocatedCluster;

        protected FileSystemInfo()
        {
            freeClusters = -1;
            lastAllocatedCluster = -1;
        }

        public int FreeClusters
        {
            get
            {
                return freeClusters;
            }
            set
            {
                freeClusters = value;
            }
        }
        public int LastAllocatedCluster
        {
            get
            {
                return lastAllocatedCluster;
            }
            set
            {
                lastAllocatedCluster = value;
            }
        }

        public abstract void Flush( Stream output );
    }
}