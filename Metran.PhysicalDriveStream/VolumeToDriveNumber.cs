// -----------------------------------------------------------------------
// <copyright file="VolumeToDriveNumber.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Metran.LowLevelAccess;
using Metran.LowLevelAccess.UnmanagedMemory;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Provides the method to map a volume identifier onto a physical drive number
    /// </summary>
    public static class VolumeToDriveNumber
    {
        /// <summary>
        /// Returns the physical drive number corresponding to the specified volume identifier
        /// </summary>
        /// <param name="volumeIdentifier">The volume identifier for which to return the physical drive number</param>
        /// <returns>The physical drive number corresponding to the specified volume identifier</returns>
        public static int Map(string volumeIdentifier)
        {
            int physicalDriveNumber;

            // build the full name
            var fullVolumeName = $"\\\\.\\{volumeIdentifier.TrimEnd(':', '\\')}:";

            // try to open the volume
            var nativeHandle = Kernel32.CreateFile(
                fullVolumeName,
                0,
                0,
                IntPtr.Zero,
                Kernel32.OpenExisting,
                Kernel32.FileAttributeNormal,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            // create a handle wrapper...
            using (var volumeHandle = new SafeFileHandle(nativeHandle, true))
            {
                // ...and check its validity
                if (volumeHandle.IsInvalid)
                {
                    throw new Win32Exception(
                        lastError,
                        $"Failed to open volume {volumeIdentifier}. Error code: {lastError}");
                }

                // try to retrieve and assign the result
                var storageDeviceNumber = RetrieveDeviceNumber(volumeHandle.DangerousGetHandle());
                physicalDriveNumber = storageDeviceNumber.DeviceNumber;
            }

            return physicalDriveNumber;
        }

        /// <summary>
        /// Returns the StorageDeviceNumber struct for the specified volume handle
        /// </summary>
        /// <param name="volumeHandle">The volume handle for which to return the StorageDeviceNumber struct</param>
        /// <returns>The StorageDeviceNumber struct for the specified volume handle</returns>
        private static StorageDeviceNumber RetrieveDeviceNumber(IntPtr volumeHandle)
        {
            StorageDeviceNumber storageDeviceNumber;

            // allocate memory for one structure
            using (var alloc = new CoTaskMemoryAllocator(Marshal.SizeOf(typeof(StorageDeviceNumber))))
            {
                int bytesRead;

                var controlResult = Kernel32.DeviceIoControl(
                    volumeHandle,
                    WinIoCtl.IoctlStorageGetDeviceNumber,
                    IntPtr.Zero,
                    0,
                    alloc.BufferAddress,
                    alloc.BufferLength,
                    out bytesRead,
                    IntPtr.Zero);
                var lastError = Marshal.GetLastWin32Error();

                // check whether the API succeeded
                if (!controlResult)
                {
                    throw new Win32Exception(
                        lastError,
                        $"Failed to retrieve the device number for the volume. Error code: {lastError}");
                }

                storageDeviceNumber = (StorageDeviceNumber) Marshal.PtrToStructure(
                    alloc.BufferAddress,
                    typeof(StorageDeviceNumber));
            }

            return storageDeviceNumber;
        }
    }
}