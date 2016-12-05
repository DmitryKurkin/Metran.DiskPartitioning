// -----------------------------------------------------------------------
// <copyright file="PhysicalDriveStream.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Metran.LowLevelAccess;
using Metran.LowLevelAccess.UnmanagedMemory;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Exposes a System.IO.Stream around a physical drive, supporting read and write operations
    /// </summary>
    public class PhysicalDriveStream : Stream
    {
        private const int BlocksMultiplier = 16;

        public static Stream OpenBuffered(int driveNumber, out DriveGeometry driveGeometry)
        {
            var driveStream = new PhysicalDriveStream(driveNumber);

            driveGeometry = new DriveGeometry(
                driveStream.DriveSize,
                driveStream.MediaType,
                driveStream.BytesPerSector,
                driveStream.Cylinders,
                driveStream.SectorsPerTrack,
                driveStream.TracksPerCylinder);

            var bufferedStream = new InputOutputBufferedStream(
                new PhysicalDriveStreamBlockDevice(driveStream),
                new ByteListPipeBuffer(),
                BlocksMultiplier);

            return bufferedStream;
        }

        public static Stream OpenBuffered(int driveNumber)
        {
            DriveGeometry driveGeometry;

            return OpenBuffered(driveNumber, out driveGeometry);
        }

        private readonly SafeFileHandle _driveHandle;

        private readonly MemoryAllocator _pageAlignedBuffer;

        private DiskGeometryEx _driveGeometry;

        public PhysicalDriveStream(int driveNumber)
        {
            var driveName = $"\\\\.\\PhysicalDrive{driveNumber}";

            var nativeHandle = Kernel32.CreateFile(
                driveName,
                Kernel32.GenericRead | Kernel32.GenericWrite,
                Kernel32.FileShareRead | Kernel32.FileShareWrite,
                IntPtr.Zero,
                Kernel32.OpenExisting,
                Kernel32.FileFlagNoBuffering | Kernel32.FileFlagWriteThrough,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            _driveHandle = new SafeFileHandle(nativeHandle, true);
            if (_driveHandle.IsInvalid)
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to open physical drive {driveNumber}. Error code: {lastError}");
            }

            RetrieveDriveGeometry();

            // we start from a buffer that is equal to the sector size
            _pageAlignedBuffer = new VirtualMemoryAllocator(BytesPerSector);
        }

        public override bool CanRead => !_driveHandle.IsClosed;

        public override bool CanSeek => !_driveHandle.IsClosed;

        public override bool CanWrite => !_driveHandle.IsClosed;

        public override long Length => DriveSize;

        public override long Position
        {
            get
            {
                AssertNotClosed();

                return Seek(0, SeekOrigin.Current);
            }

            set
            {
                AssertNotClosed();

                Seek(value, SeekOrigin.Begin);
            }
        }

        public long DriveSize
        {
            get
            {
                AssertNotClosed();

                return _driveGeometry.DiskSize;
            }
        }

        public int MediaType
        {
            get
            {
                AssertNotClosed();

                return _driveGeometry.Geometry.MediaType;
            }
        }

        public int BytesPerSector
        {
            get
            {
                AssertNotClosed();

                return _driveGeometry.Geometry.BytesPerSector;
            }
        }

        public long Cylinders
        {
            get
            {
                AssertNotClosed();

                return _driveGeometry.Geometry.Cylinders;
            }
        }

        public int SectorsPerTrack
        {
            get
            {
                AssertNotClosed();

                return _driveGeometry.Geometry.SectorsPerTrack;
            }
        }

        public int TracksPerCylinder
        {
            get
            {
                AssertNotClosed();

                return _driveGeometry.Geometry.TracksPerCylinder;
            }
        }

        public override void Close()
        {
            AssertNotClosed();

            _driveHandle.Dispose();
            _pageAlignedBuffer.Dispose();

            base.Close();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            AssertNotClosed();
            Validate(buffer, offset, count);

            EnsureBufferCapacity(count);

            int bytesRead;
            var result = Kernel32.ReadFile(
                _driveHandle.DangerousGetHandle(),
                _pageAlignedBuffer.BufferAddress,
                count,
                out bytesRead,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            if (result)
            {
                // we expect the buffer to contain "bytesRead" bytes
                _pageAlignedBuffer.Read(buffer, offset, bytesRead);
            }
            else
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to read from the physical drive. Error code: {lastError}");
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            AssertNotClosed();

            if (offset%BytesPerSector != 0)
            {
                throw new ArgumentException(
                    $"Drive access sizes must be for a number of bytes that is an integer multiple of the sector size. The sector size is {BytesPerSector}. The specified offset is {offset}");
            }

            long newFilePointer;
            var result = Kernel32.SetFilePointerEx(
                _driveHandle.DangerousGetHandle(),
                offset,
                out newFilePointer,
                origin);
            var lastError = Marshal.GetLastWin32Error();

            if (!result)
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to seek the physical drive. Error code: {lastError}");
            }

            return newFilePointer;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertNotClosed();
            Validate(buffer, offset, count);

            EnsureBufferCapacity(count);

            // put the passed data to the buffer
            _pageAlignedBuffer.Write(buffer, offset, count);

            int bytesWritten;
            var result = Kernel32.WriteFile(
                _driveHandle.DangerousGetHandle(),
                _pageAlignedBuffer.BufferAddress,
                count,
                out bytesWritten,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            if (!result)
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to write to the physical drive. Error code: {lastError}");
            }

            // we don't check how many bytes were actually written
        }

        private void AssertNotClosed()
        {
            if (_driveHandle.IsClosed)
            {
                throw new ObjectDisposedException(
                    "driveHandle",
                    "The physical drive stream has been closed");
            }
        }

        private void Validate(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException(
                    $"The sum of offset and count ({offset + count}) is larger than the buffer length ({buffer.Length})");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(offset),
                    $"The offset is negative ({offset})");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    $"The count is negative ({count})");
            }

            if (count%BytesPerSector != 0)
            {
                throw new ArgumentException(
                    $"Drive access sizes must be for a number of bytes that is an integer multiple of the sector size. The sector size is {BytesPerSector}. The specified count is {count}");
            }
        }

        private void RetrieveDriveGeometry()
        {
            using (var allocator = new CoTaskMemoryAllocator(Marshal.SizeOf(typeof(DiskGeometryEx))))
            {
                int bytesRead;

                var result = Kernel32.DeviceIoControl(
                    _driveHandle.DangerousGetHandle(),
                    WinIoCtl.IoctlDiskGetDriveGeometryEx,
                    IntPtr.Zero,
                    0,
                    allocator.BufferAddress,
                    allocator.BufferLength,
                    out bytesRead,
                    IntPtr.Zero);
                var lastError = Marshal.GetLastWin32Error();

                if (result)
                {
                    _driveGeometry = (DiskGeometryEx) Marshal.PtrToStructure(
                        allocator.BufferAddress,
                        typeof(DiskGeometryEx));
                }
                else
                {
                    throw new Win32Exception(
                        lastError,
                        $"Failed to get the drive's geometry. Error code: {lastError}");
                }
            }
        }

        private void EnsureBufferCapacity(int requiredNumberOfBytes)
        {
            if (_pageAlignedBuffer.BufferLength < requiredNumberOfBytes)
            {
                _pageAlignedBuffer.ReAllocate(requiredNumberOfBytes);
            }
        }
    }
}