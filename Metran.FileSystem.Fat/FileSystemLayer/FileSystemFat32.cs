using Metran.FileSystem.Fat.ClusterChainLayer;
using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.ClusterLayer;
using Metran.FileSystem.Fat.VFATLayer;
using System.IO;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Wraps a stream and implements the FAT32 file system over it. Considers the size of a sector to be 512 bytes
    /// </summary>
    /// <remarks>The implementation is really missing the Dependency Injection framework. It should be using plain interfaces and not classes</remarks>
    public class FileSystemFat32 : FileSystemFatBase
    {
        private const ushort PredefinedReservedSectorsCount = 32;
        private const byte PredefinedFatsCount = 2;
        private const byte PredefinedRootDirectoryEntriesCount = 0;

        private static readonly VolumeSizeToSectorsPerCluster[] VolumeTable =
        {
            new VolumeSizeToSectorsPerCluster(66600, InvalidSectorsPerCluster),
            // disks up to 32.5 MB, the 0 value for SectorsPerCluster trips an error
            new VolumeSizeToSectorsPerCluster(532480, 1), // disks up to 260 MB, .5k cluster
            new VolumeSizeToSectorsPerCluster(16777216, 8), // disks up to 8 GB, 4k cluster
            new VolumeSizeToSectorsPerCluster(33554432, 16), // disks up to 16 GB, 8k cluster
            new VolumeSizeToSectorsPerCluster(67108864, 32), // disks up to 32 GB, 16k cluster
            new VolumeSizeToSectorsPerCluster(0xFFFFFFFF, 64) // disks greater than 32GB, 32k cluster
        };

        private static uint ComputeFatSectors(
            uint volumeSectorsCount,
            ushort reservedSectorsCount,
            byte fatsCount,
            byte sectorsPerCluster)
        {
            // just according to the spec...

            var tmpVal1 = volumeSectorsCount - reservedSectorsCount;
            var tmpVal2 = (uint) (256*sectorsPerCluster + fatsCount);

            tmpVal2 /= 2;

            var fatSectors = (tmpVal1 + (tmpVal2 - 1))/tmpVal2;

            return fatSectors;
        }

        private ExtendedBiosParameterBlockFat32 _ebpb;

        private IFileSystemInformation _fsInfo;

        private RootDirectoryFat32 _rootDirectory;

        public FileSystemFat32(Stream targetStream, VolumeStaticInfo volumeStaticInfo)
            : base(targetStream, volumeStaticInfo)
        {
        }

        public FileSystemFat32(Stream targetStream)
            : base(targetStream)
        {
        }

        public override IDirectory RootDirectory => _rootDirectory;

        public override void Load()
        {
            // create and load the BPB and EBPB (don't position the stream, load from the current position)
            Bpb = new BiosParameterBlock(TargetStream);
            _ebpb = new ExtendedBiosParameterBlockFat32(TargetStream);

            // copy data from the BPB and EBPB to the static and dynamic parameters
            LoadVolumeParameters();

            // create and load the FSInfo
            PositionToFsInfo();
            _fsInfo = new FileSystemInformation(TargetStream);

            // create and load the first FAT
            PositionToFatRegion();
            Fat = new FileAllocationTableFat32(TargetStream, ClustersCount, _fsInfo);

            // create the Data Region
            DataRegion = new DataRegion(
                TargetStream,
                DataRegionStartSector,
                ClustersCount,
                VolumeDynamicInfo.SectorsPerCluster);

            // create the Root Directory
            _rootDirectory = new RootDirectoryFat32(
                (int) _ebpb.RootDirectoryFirstCluster,
                new ClusterChainStreamManager(new ClusterChainManager(Fat, DataRegion)),
                new DirectoryEntryManager());

            // load the root dir (we don't call the FinishCreation because all data already exist,
            // we just need to read it, there is nothing to create)
            _rootDirectory.LoadRecursive();
        }

        public override void Format()
        {
            ComputeDynamicParameters();
            ClearSystemArea();

            // use the static and dynamic parameters, all the others get set to their default values

            // create and setup the BPB
            Bpb = new BiosParameterBlock
            {
                BytesPerSector = VolumeStaticInfo.BytesPerSector,
                HiddenSectorsCount = VolumeStaticInfo.StartSector,
                SectorsPerTrack = VolumeStaticInfo.SectorsPerTrack,
                TracksPerCylinder = VolumeStaticInfo.TracksPerCylinder,
                VolumeSectorsCount = VolumeStaticInfo.SectorsCount,
                FatsCount = VolumeDynamicInfo.FatsCount,
                ReservedSectorsCount = VolumeDynamicInfo.ReservedSectorsCount,
                RootDirectoryEntriesCount = VolumeDynamicInfo.RootDirectoryEntriesCount,
                SectorsPerCluster = VolumeDynamicInfo.SectorsPerCluster
            };

            // create and setup the EBPB
            _ebpb = new ExtendedBiosParameterBlockFat32
            {
                FatSectorsCount = VolumeDynamicInfo.FatSectorsCount,
                VolumeSerialNumber = GenerateVolumeSerialNumber()
            };

            // create the FSInfo
            _fsInfo = new FileSystemInformation();

            // create the FAT
            Fat = new FileAllocationTableFat32(ClustersCount, Bpb.MediaDescriptor, _fsInfo);

            // create the Data Region
            DataRegion = new DataRegion(TargetStream, DataRegionStartSector, ClustersCount, Bpb.SectorsPerCluster);

            // create the Root Directory
            _rootDirectory = new RootDirectoryFat32(
                new ClusterChainStreamManager(new ClusterChainManager(Fat, DataRegion)),
                new DirectoryEntryManager());

            // let the Root Directory create its internal structure
            _rootDirectory.FinishCreation();

            // save the root dir cluster in the EBPB
            _ebpb.RootDirectoryFirstCluster = (uint) _rootDirectory.FirstCluster;
        }

        public override void Flush()
        {
            PositionToReservedRegion();
            Bpb.Save(TargetStream);
            _ebpb.Save(TargetStream);

            PositionToFsInfo();
            _fsInfo.Save(TargetStream);

            PositionToReservedRegionBackup();
            Bpb.Save(TargetStream);
            _ebpb.Save(TargetStream);

            PositionToFsInfoBackup();
            _fsInfo.Save(TargetStream);

            PositionToFatRegion();
            for (var i = 0; i < Bpb.FatsCount; i++)
            {
                Fat.Save(TargetStream);
            }

            // the Root Dir Region doesn't exist and the Data Region is considered to be up to date (it gets flushed automatically during its usage)
        }

        public override string VolumeLabel
        {
            get
            {
                var volumeLabel = string.Empty;

                if (_rootDirectory.VolumeLabelExists)
                {
                    volumeLabel = _rootDirectory.VolumeLabel;
                }

                return volumeLabel;
            }
        }

        public override void AssignVolumeLabel(string volumeLabel)
        {
            // create a label in the root directory
            _rootDirectory.AssignVolumeLabel(volumeLabel);

            // load it into the EBPB
            _ebpb.VolumeLabel = _rootDirectory.VolumeLabel;
        }

        public override void DeleteVolumeLabel()
        {
            // delete it from the root directory
            _rootDirectory.DeleteVolumeLabel();

            // load a default one into the EBPB
            _ebpb.VolumeLabel = ExtendedBiosParameterBlock.VolumeLabelDefault;
        }

        private void ComputeDynamicParameters()
        {
            VolumeDynamicInfo.ReservedSectorsCount = PredefinedReservedSectorsCount;
            VolumeDynamicInfo.FatsCount = PredefinedFatsCount;
            VolumeDynamicInfo.RootDirectoryEntriesCount = PredefinedRootDirectoryEntriesCount;

            VolumeDynamicInfo.SectorsPerCluster = ComputeSectorsPerCluster(VolumeTable, VolumeStaticInfo.SectorsCount);
            VolumeDynamicInfo.FatSectorsCount = ComputeFatSectors(
                VolumeStaticInfo.SectorsCount,
                VolumeDynamicInfo.ReservedSectorsCount,
                VolumeDynamicInfo.FatsCount,
                VolumeDynamicInfo.SectorsPerCluster);
        }

        private void LoadVolumeParameters()
        {
            VolumeStaticInfo.BytesPerSector = Bpb.BytesPerSector;
            VolumeStaticInfo.SectorsCount = Bpb.VolumeSectorsCount;
            VolumeStaticInfo.SectorsPerTrack = Bpb.SectorsPerTrack;
            VolumeStaticInfo.StartSector = Bpb.HiddenSectorsCount;
            VolumeStaticInfo.TracksPerCylinder = Bpb.TracksPerCylinder;

            VolumeDynamicInfo.ReservedSectorsCount = Bpb.ReservedSectorsCount;
            VolumeDynamicInfo.FatsCount = Bpb.FatsCount;
            VolumeDynamicInfo.RootDirectoryEntriesCount = Bpb.RootDirectoryEntriesCount;
            VolumeDynamicInfo.SectorsPerCluster = Bpb.SectorsPerCluster;
            VolumeDynamicInfo.FatSectorsCount = _ebpb.FatSectorsCount;
        }

        private void PositionToFsInfo()
        {
            TargetStream.Position = (ReservedRegionStartSector + _ebpb.FileSystemInfoSector) *
                                    VolumeStaticInfo.BytesPerSector;
        }

        private void PositionToReservedRegionBackup()
        {
            TargetStream.Position = (ReservedRegionStartSector + _ebpb.BackupSector) * VolumeStaticInfo.BytesPerSector;
        }

        private void PositionToFsInfoBackup()
        {
            TargetStream.Position = (ReservedRegionStartSector + _ebpb.BackupSector + _ebpb.FileSystemInfoSector) *
                                    VolumeStaticInfo.BytesPerSector;
        }
    }
}