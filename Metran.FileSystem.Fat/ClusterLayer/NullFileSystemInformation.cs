using Metran.FileSystem.Fat.ClusterLayer;
using System.IO;

namespace Metran.FileSystem.Fat
{
    /// <summary>
    /// Represents a file system information with no backing store
    /// </summary>
    public class NullFileSystemInformation : IFileSystemInformation
    {
        int IFileSystemInformation.FreeClusters
        {
            get { return -1; }
            set { }
        }

        int IFileSystemInformation.LastAllocatedCluster
        {
            get { return -1; }
            set { }
        }

        void IFileSystemInformation.Save(Stream output)
        {
        }
    }
}