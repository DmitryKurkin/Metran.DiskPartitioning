// -----------------------------------------------------------------------
// <copyright file="PhysicalDriveStreamBlockDevice.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Wraps a physical drive stream into a block device
    /// </summary>
    internal class PhysicalDriveStreamBlockDevice : IBlockDevice
    {
        private readonly PhysicalDriveStream _driveStream;

        public PhysicalDriveStreamBlockDevice(PhysicalDriveStream driveStream)
        {
            if (driveStream == null) throw new ArgumentNullException(nameof(driveStream));

            _driveStream = driveStream;
        }

        int IBlockDevice.BlockSize => _driveStream.BytesPerSector;

        bool IBlockDevice.SupportsPositioning => _driveStream.CanSeek;

        bool IBlockDevice.SupportsReading => _driveStream.CanRead;

        bool IBlockDevice.SupportsWriting => _driveStream.CanWrite;

        long IBlockDevice.Position(long value)
        {
            return _driveStream.Seek(value, SeekOrigin.Begin);
        }

        byte[] IBlockDevice.ReadBlock()
        {
            var sectorBytes = new byte[_driveStream.BytesPerSector];

            var bytesRead = _driveStream.Read(sectorBytes, 0, sectorBytes.Length);

            // handle a possible end of the stream
            if (bytesRead == 0)
            {
                sectorBytes = new byte[0];
            }

            return sectorBytes;
        }

        void IBlockDevice.WriteBlock(byte[] blockData)
        {
            _driveStream.Write(blockData, 0, blockData.Length);
        }

        void IBlockDevice.Close()
        {
            _driveStream.Close();
        }
    }
}