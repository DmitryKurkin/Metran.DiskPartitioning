// -----------------------------------------------------------------------
// <copyright file="VolumeLocker.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Metran.LowLevelAccess;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Locks and dismounts volumes
    /// </summary>
    public class VolumeLocker : IDisposable
    {
        public static VolumeLocker Lock(string volumeIdentifier)
        {
            return new VolumeLocker(volumeIdentifier, true, false);
        }

        public static VolumeLocker Dismount(string volumeIdentifier)
        {
            return new VolumeLocker(volumeIdentifier, false, true);
        }

        public static VolumeLocker LockAndDismount(string volumeIdentifier)
        {
            return new VolumeLocker(volumeIdentifier, true, true);
        }

        private readonly SafeFileHandle _volumeHandle;

        private bool _isLocked;

        private VolumeLocker(string volumeIdentifier, bool lockVolume, bool dismountVolume)
        {
            var volumeName = $"\\\\.\\{volumeIdentifier.TrimEnd(':', '\\')}:";

            // not sure if anything instead of Kernel32.GenericRead is required...
            var nativeHandle = Kernel32.CreateFile(
                volumeName,
                Kernel32.GenericRead,
                Kernel32.FileShareRead | Kernel32.FileShareWrite,
                IntPtr.Zero,
                Kernel32.OpenExisting,
                Kernel32.FileAttributeNormal,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            _volumeHandle = new SafeFileHandle(nativeHandle, true);
            if (_volumeHandle.IsInvalid)
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to open volume {volumeIdentifier}. Error code: {lastError}");
            }

            // do what we are being asked...
            ////

            if (lockVolume)
            {
                DoLock();
            }

            if (dismountVolume)
            {
                DoDismount();
            }
        }

        public void Dispose()
        {
            // there is no "mount" call, so just unlock if it needs...
            ////

            if (_isLocked)
            {
                DoUnlock();
            }

            _volumeHandle.Dispose();
        }

        private void DoLock()
        {
            int bytesReturned;
            var result = Kernel32.DeviceIoControl(
                _volumeHandle.DangerousGetHandle(),
                WinIoCtl.FsctlLockVolume,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                out bytesReturned,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            if (!result)
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to lock the volume. Error code: {lastError}");
            }

            _isLocked = true;
        }

        private void DoUnlock()
        {
            int bytesReturned;
            var result = Kernel32.DeviceIoControl(
                _volumeHandle.DangerousGetHandle(),
                WinIoCtl.FsctlUnlockVolume,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                out bytesReturned,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            if (!result)
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to unlock the volume. Error code: {lastError}");
            }

            _isLocked = false;
        }

        private void DoDismount()
        {
            int bytesReturned;
            var result = Kernel32.DeviceIoControl(
                _volumeHandle.DangerousGetHandle(),
                WinIoCtl.FsctlDismountVolume,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                out bytesReturned,
                IntPtr.Zero);
            var lastError = Marshal.GetLastWin32Error();

            if (!result)
            {
                throw new Win32Exception(
                    lastError,
                    $"Failed to dismount the volume. Error code: {lastError}");
            }
        }
    }
}