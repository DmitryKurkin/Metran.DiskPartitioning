using System.IO;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Defines properties for storing and retrieving information about file system clusters usage
    /// </summary>
    public interface IFileSystemInformation
    {
        int FreeClusters { get; set; }

        int LastAllocatedCluster { get; set; }

        void Save(Stream output);
    }
}