using Metran.FileSystem.Fat.FileSystemLayer;
using Metran.IO.Streams;
using Metran.LowLevelAccess;
using System.IO;
using PhysicalDriveStream = Metran.IO.Streams.PhysicalDriveStream;

namespace Test.FatErrors
{
    class Program
    {
        static void Main(string[] args)
        {
            string volumeName = "h";

            using (VolumeLocker.LockAndDismount(volumeName))
            {
                int driveNumber = VolumeToDriveNumber.Map(volumeName);

                DriveLayoutInformationEx layoutInfo = PhysicalDriveManager.GetDriveLayoutInformation(driveNumber);

                DriveGeometry driveGeometry;
                var diskStream = PhysicalDriveStream.OpenBuffered(driveNumber, out driveGeometry);

                diskStream.Seek(layoutInfo.PartitionEntries[0].StartingOffset, SeekOrigin.Begin);

                using (var fileSystem = new FileSystemFat32(diskStream))
                {
                    var file = fileSystem.RootDirectory.CreateFile("1.txt");
                    using (var stream = file.OpenWrite())
                    {
                        stream.Write(new byte[4096 * 2], 0, 4096 * 2);
                    }
                }
            }
        }
    }
}