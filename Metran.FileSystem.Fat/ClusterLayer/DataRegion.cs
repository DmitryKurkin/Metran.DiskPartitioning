using System;
using System.IO;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Implements a Data Region over a stream. Considers the size of a sector to be 512 bytes
    /// </summary>
    public class DataRegion : IDataRegion
    {
        public const int BytesPerSector = 512;

        private const int MaxSectorsPerCluster = 1024;

        private readonly Stream _targetStream;

        private readonly long _startSector;

        private readonly int _clustersCount;

        private readonly int _sectorsPerCluster;

        public DataRegion(Stream targetStream, long startSector, int clustersCount, int sectorsPerCluster)
        {
            if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));

            if (!targetStream.CanRead)
            {
                throw new ArgumentException("The target stream must be readable", nameof(targetStream));
            }

            if (!targetStream.CanSeek)
            {
                throw new ArgumentException("The target stream must be seekable", nameof(targetStream));
            }

            if (!targetStream.CanWrite)
            {
                throw new ArgumentException("The target stream must be writeable", nameof(targetStream));
            }

            if (startSector < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startSector),
                    $"The start sector is negative: {startSector}");
            }

            if (clustersCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(clustersCount),
                    $"The clusters count must be greater than 1. The specified value is {clustersCount}");
            }

            if (sectorsPerCluster < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sectorsPerCluster),
                    $"The sectors per cluster count must be greater than 1. The specified value is {sectorsPerCluster}");
            }

            if (sectorsPerCluster > MaxSectorsPerCluster)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sectorsPerCluster),
                    $"The sectors per cluster count must be less or equal to {MaxSectorsPerCluster}. The specified value is {sectorsPerCluster}");
            }

            _targetStream = targetStream;
            _startSector = startSector;

            _clustersCount = clustersCount;
            _sectorsPerCluster = sectorsPerCluster;
        }

        int IDataRegion.ClustersCount => _clustersCount;

        int IDataRegion.SectorsPerCluster => _sectorsPerCluster;

        int IDataRegion.BytesPerSector => BytesPerSector;

        byte[] IDataRegion.ReadCluster(int cluster)
        {
            if (cluster < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cluster),
                    $"The cluster number is negative: {cluster}");
            }

            if (cluster >= _clustersCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cluster),
                    $"The cluster number ({cluster}) must be less than the total clusters count ({_clustersCount})");
            }

            // go to the start of the cluster
            PositionStream(cluster);

            // read data from it (even if the stream returns zero bytes, we return a whole cluster)
            var clusterData = new byte[_sectorsPerCluster*BytesPerSector];
            _targetStream.Read(clusterData, 0, clusterData.Length);

            return clusterData;
        }

        void IDataRegion.WriteCluster(int cluster, byte[] clusterData)
        {
            if (clusterData == null) throw new ArgumentNullException(nameof(clusterData));

            if (cluster < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cluster),
                    $"The cluster number is negative: {cluster}");
            }

            if (cluster >= _clustersCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cluster),
                    $"The cluster number ({cluster}) must be less than the total clusters count ({_clustersCount})");
            }

            if (clusterData.Length%(_sectorsPerCluster*BytesPerSector) != 0)
            {
                throw new ArgumentException(
                    $"The cluster data length ({clusterData.Length}) must be an integer multiple of a single cluster length ({_sectorsPerCluster*BytesPerSector})",
                    nameof(clusterData));
            }

            // go to the start of the cluster
            PositionStream(cluster);

            // write the data to it
            _targetStream.Write(clusterData, 0, clusterData.Length);
        }

        private void PositionStream(int cluster)
        {
            // a byte-based position of the cluster
            _targetStream.Position = (_startSector + cluster*_sectorsPerCluster)*BytesPerSector;
        }
    }
}