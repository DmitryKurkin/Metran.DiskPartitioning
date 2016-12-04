using Metran.FileSystem.Fat.ClusterChainLayer;
using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.ClusterLayer;
using Metran.FileSystem.Fat.VFATLayer;
using System.IO;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Wraps a stream and implements the FAT16 file system over it. Considers the size of a sector to be 512 bytes
    /// </summary>
    public class FileSystemFat16 : FileSystemFatBase
    {
        private const ushort PredefinedReservedSectorsCount = 1;
        private const byte PredefinedFatsCount = 2;
        private const ushort PredefinedRootDirectoryEntriesCount = 512;

        private static readonly VolumeSizeToSectorsPerCluster[] VolumeTable =
        {
            new VolumeSizeToSectorsPerCluster(8400, InvalidSectorsPerCluster),
            // disks up to 4.1 MB, the 0 value for SectorsPerCluster trips an error
            new VolumeSizeToSectorsPerCluster(32680, 2), // disks up to 16 MB, 1k cluster
            new VolumeSizeToSectorsPerCluster(262144, 4), // disks up to 128 MB, 2k cluster
            new VolumeSizeToSectorsPerCluster(524288, 8), // disks up to 256 MB, 4k cluster
            new VolumeSizeToSectorsPerCluster(1048576, 16), // disks up to 512 MB, 8k cluster
            // The entries after this point are not used unless FAT16 is forced
            new VolumeSizeToSectorsPerCluster(2097152, 32), // disks up to 1 GB, 16k cluster
            new VolumeSizeToSectorsPerCluster(4194304, 64), // disks up to 2 GB, 32k cluster
            new VolumeSizeToSectorsPerCluster(0xFFFFFFFF, InvalidSectorsPerCluster)
            // any disk greater than 2GB, 0 value for SectorsPerCluster trips an error
        };

        private static uint ComputeFatSectors(
            uint volumeSectorsCount,
            ushort reservedSectorsCount,
            byte fatsCount,
            int rootDirSectorsCount,
            byte sectorsPerCluster)
        {
            // just according to the spec...

            var tmpVal1 = volumeSectorsCount - reservedSectorsCount - (uint) rootDirSectorsCount;
            var tmpVal2 = (uint) (256*sectorsPerCluster + fatsCount);

            var fatSectors = (tmpVal1 + (tmpVal2 - 1))/tmpVal2;

            return fatSectors;
        }

        private ExtendedBiosParameterBlock _ebpb;

        private RootDirectoryFat16 _rootDirectory;

        public FileSystemFat16(Stream targetStream, VolumeStaticInfo volumeStaticInfo)
            : base(targetStream, volumeStaticInfo)
        {
        }

        public FileSystemFat16(Stream targetStream)
            : base(targetStream)
        {
        }

        public override IDirectory RootDirectory => _rootDirectory;

        public override void Load()
        {
            // create and load the BPB and EBPB (don't position the stream, load from the current position)
            Bpb = new BiosParameterBlock(TargetStream);
            _ebpb = new ExtendedBiosParameterBlock(TargetStream);

            // copy data from the BPB and EBPB to the static and dynamic parameters
            LoadVolumeParameters();

            // create and load the first FAT
            PositionToFatRegion();
            Fat = new FileAllocationTableFat16(TargetStream, ClustersCount, new NullFileSystemInformation());

            // create the Data Region
            DataRegion = new DataRegion(TargetStream, DataRegionStartSector, ClustersCount,
                VolumeDynamicInfo.SectorsPerCluster);

            // create the Root Directory
            _rootDirectory = new RootDirectoryFat16(
                TargetStream,
                RootDirRegionStartSector,
                RootDirRegionSectorsCount,
                new ClusterChainStreamManager(new ClusterChainManager(Fat, DataRegion)),
                new DirectoryEntryManager());

            // load the Root Directory (we don't call the FinishCreation because all data already exist,
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
                FatSectorsCount = (ushort) VolumeDynamicInfo.FatSectorsCount,
                ReservedSectorsCount = VolumeDynamicInfo.ReservedSectorsCount,
                RootDirectoryEntriesCount = VolumeDynamicInfo.RootDirectoryEntriesCount,
                SectorsPerCluster = VolumeDynamicInfo.SectorsPerCluster
            };

            // create and setup the EBPB
            _ebpb = new ExtendedBiosParameterBlock {VolumeSerialNumber = GenerateVolumeSerialNumber()};

            // create the FAT
            Fat = new FileAllocationTableFat16(ClustersCount, Bpb.MediaDescriptor, new NullFileSystemInformation());

            // create the Data Region
            DataRegion = new DataRegion(TargetStream, DataRegionStartSector, ClustersCount, Bpb.SectorsPerCluster);

            // create the Root Directory
            _rootDirectory = new RootDirectoryFat16(
                TargetStream,
                RootDirRegionStartSector,
                RootDirRegionSectorsCount,
                new ClusterChainStreamManager(new ClusterChainManager(Fat, DataRegion)),
                new DirectoryEntryManager());

            // let the Root Directory create its internal structure
            _rootDirectory.FinishCreation();
        }

        public override void Flush()
        {
            PositionToReservedRegion();
            Bpb.Save(TargetStream);
            _ebpb.Save(TargetStream);

            PositionToFatRegion();
            for (var i = 0; i < Bpb.FatsCount; i++)
            {
                Fat.Save(TargetStream);
            }

            // the Root Dir Region and Data Region are considered to be up to date (they get flushed automatically during their usage)
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
                RootDirRegionSectorsCount,
                // depends on volumeDynamicInfo.RootDirectoryEntriesCount, set the field first!!!
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
            VolumeDynamicInfo.FatSectorsCount = Bpb.FatSectorsCount;
        }
    }
}