using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.ClusterLayer;
using Metran.FileSystem.Fat.VFATLayer;
using System;
using System.IO;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Provides the base class for a FAT file system
    /// </summary>
    public abstract class FileSystemFatBase : IFileSystem
    {
        protected const ushort PredefinedBytesPerSector = 512;

        protected const byte InvalidSectorsPerCluster = 0;

        protected static byte ComputeSectorsPerCluster(
            VolumeSizeToSectorsPerCluster[] volumeTable,
            uint volumeSectorsCount)
        {
            byte sectorsPerCluster = 0;

            // a simple lookup using the table
            foreach (var sizeToSpc in volumeTable)
            {
                if (volumeSectorsCount <= sizeToSpc.VolumeSectorsCount)
                {
                    sectorsPerCluster = sizeToSpc.SectorsPerCluster;
                    break;
                }
            }

            // check it right away
            if (sectorsPerCluster == InvalidSectorsPerCluster)
            {
                throw new ArgumentException(
                    $"The specified volume sectors count is invalid for this file system: {volumeSectorsCount}");
            }

            return sectorsPerCluster;
        }

        private static void ValidateVolumeGeometry(VolumeStaticInfo volumeGeometry, string parameterNumber)
        {
            if (volumeGeometry.BytesPerSector != PredefinedBytesPerSector)
            {
                throw new ArgumentException(
                    $"The bytes per sector value is invalid: {volumeGeometry.BytesPerSector}. The expected value is: {PredefinedBytesPerSector}",
                    parameterNumber);
            }
        }

        protected static uint GenerateVolumeSerialNumber()
        {
            ushort date;
            ushort time;
            FatDateTime.Unpack(DateTime.Now, out date, out time);

            var serialNumber = Utils.PackToUInt32(date, time);

            return serialNumber;
        }

        protected Stream TargetStream;

        protected VolumeStaticInfo VolumeStaticInfo;

        protected VolumeDynamicInfo VolumeDynamicInfo;

        protected BiosParameterBlock Bpb;

        protected IFileAllocationTable Fat;

        protected IDataRegion DataRegion;

        protected IClusterChainStreamManager ClusterChainStreamManager;

        protected IDirectoryEntryManager DirectoryEntryManager;

        protected FileSystemFatBase(Stream targetStream, VolumeStaticInfo volumeStaticInfo)
        {
            ValidateTargetStream(targetStream, "targetStream");
            ValidateVolumeGeometry(volumeStaticInfo, "volumeStaticInfo");

            TargetStream = targetStream;
            VolumeStaticInfo = volumeStaticInfo;

            // ReSharper disable once VirtualMemberCallInConstructor
            Format();
        }

        protected FileSystemFatBase(Stream targetStream)
        {
            ValidateTargetStream(targetStream, "targetStream");

            TargetStream = targetStream;

            // ReSharper disable once VirtualMemberCallInConstructor
            Load();
        }

        #region Useful calculations over the Static and Dynamic infos

        protected int ReservedRegionSectorsCount => VolumeDynamicInfo.ReservedSectorsCount;

        protected int FatRegionSectorsCount => (int) VolumeDynamicInfo.FatSectorsCount*VolumeDynamicInfo.FatsCount;

        protected int RootDirRegionSectorsCount
        {
            get
            {
                // find a better way of getting the size of an entry?
                var rootDirSize = VolumeDynamicInfo.RootDirectoryEntriesCount*DirectoryEntryParser.DirectoryEntryLength;

                // round up to a whole sectors number
                var rootDirSectors = (rootDirSize + (VolumeStaticInfo.BytesPerSector - 1))/
                                     VolumeStaticInfo.BytesPerSector;

                return rootDirSectors;
            }
        }

        // 3 regions...
        protected int SystemRegionSectorsCount
            => ReservedRegionSectorsCount + FatRegionSectorsCount + RootDirRegionSectorsCount;

        protected int ReservedRegionStartSector => (int) VolumeStaticInfo.StartSector;

        protected int FatRegionStartSector => ReservedRegionStartSector + ReservedRegionSectorsCount;

        protected int RootDirRegionStartSector
            => ReservedRegionStartSector + ReservedRegionSectorsCount + FatRegionSectorsCount;

        protected int DataRegionStartSector
            =>
            ReservedRegionStartSector + ReservedRegionSectorsCount + FatRegionSectorsCount + RootDirRegionSectorsCount;

        protected int ClustersCount
        {
            get
            {
                var dataSectors = VolumeStaticInfo.SectorsCount - SystemRegionSectorsCount;

                // round down to a whole clusters number
                var clusters = (int) (dataSectors/VolumeDynamicInfo.SectorsPerCluster);

                return clusters;
            }
        }

        #endregion

        protected void PositionToReservedRegion()
        {
            // just go to the volume start sector
            TargetStream.Position = ReservedRegionStartSector*VolumeStaticInfo.BytesPerSector;
        }

        protected void PositionToFatRegion()
        {
            TargetStream.Position = FatRegionStartSector*VolumeStaticInfo.BytesPerSector;
        }

        protected void PositionToRootDirectoryRegion()
        {
            TargetStream.Position = RootDirRegionStartSector*VolumeStaticInfo.BytesPerSector;
        }

        protected void ClearSystemArea()
        {
            // go to the start sector of the volume
            PositionToReservedRegion();

            // zero data to be written
            var zeroes = new byte[VolumeStaticInfo.BytesPerSector];

            // clear sectors
            for (var i = 0; i < SystemRegionSectorsCount; i++)
            {
                TargetStream.Write(zeroes, 0, zeroes.Length);
            }
        }

        private void ValidateTargetStream(Stream targetStream, string parameterNumber)
        {
            if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));

            if (!targetStream.CanRead)
            {
                throw new ArgumentException("The target stream must be readable", parameterNumber);
            }

            if (!targetStream.CanSeek)
            {
                throw new ArgumentException("The target stream must be seekable", parameterNumber);
            }

            if (!targetStream.CanWrite)
            {
                throw new ArgumentException("The target stream must be writeable", parameterNumber);
            }
        }

        public abstract IDirectory RootDirectory { get; }

        public abstract void Load();

        public abstract void Format();

        public abstract void Flush();

        public virtual void Dispose()
        {
            // actually, this is not a real dispose.
            // it was done to be able to use the file system with the using statement

            Flush();
        }

        public abstract string VolumeLabel { get; }

        public abstract void AssignVolumeLabel(string volumeLabel);

        public abstract void DeleteVolumeLabel();
    }
}