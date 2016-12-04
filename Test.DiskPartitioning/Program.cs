using Metran.FileSystem;
using Metran.IO.Streams;
using System;
using System.IO;
using Metran.LowLevelAccess.FileSystem;

namespace Test.DiskPartitioning
{
    public class Program
    {
        internal static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid number of arguments");
                return;
            }

            try
            {
                var number = VolumeToDriveNumber.Map(args[0]);
                Console.WriteLine("Succeeded: " + number);

                using (VolumeLocker.LockAndDismount("i"))
                using (var s = PhysicalDriveStream.OpenBuffered(number))
                {
                    var mbr = new Metran.DiskPartitioning.MasterBootRecord(s);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed: " + e.Message);
            }
        }

        private static void ClearSectors(int drive, int sectorsCount)
        {
            using (VolumeLocker l = VolumeLocker.LockAndDismount("g"))
            {
                using (Stream s = PhysicalDriveStream.OpenBuffered(drive))
                {
                    byte[] sectorBytes = new byte[512];

                    for (int i = 0; i < sectorsCount; i++)
                    {
                        s.Write(sectorBytes, 0, sectorBytes.Length);
                    }
                }
            }
        }

        private static void DumpSectors(int drive, int sectorsCount, string path)
        {
            using (VolumeLocker l = VolumeLocker.LockAndDismount("g"))
            {
                using (Stream input = PhysicalDriveStream.OpenBuffered(drive), output = File.OpenWrite(path))
                {
                    byte[] sectorBytes = new byte[512];

                    for (int i = 0; i < sectorsCount; i++)
                    {
                        input.Read(sectorBytes, 0, sectorBytes.Length);
                        output.Write(sectorBytes, 0, sectorBytes.Length);
                    }
                }
            }
        }

        private static void PrintDirRecursively(IDirectory directory)
        {
            IDirectory[] subdirs = directory.GetDirectories();
            IFile[] files = directory.GetFiles();

            Console.WriteLine("Contents of " + directory.Name);
            foreach (IDirectory d in subdirs)
            {
                Console.WriteLine(d.Name + " " + d.Attributes);
            }
            foreach (IFile f in files)
            {
                Console.WriteLine(f.Name + " " + f.Attributes);
            }

            foreach (IDirectory d in subdirs)
            {
                PrintDirRecursively(d);
            }
        }
    }
}