// -----------------------------------------------------------------------
// <copyright file="DiskLoadingViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Gets.Utils;
using Metran.FileSystem;
using Metran.FileSystem.Fat.FileSystemLayer;
using Metran.IO.Streams;
using Metran.LowLevelAccess;
using System;
using System.ComponentModel;
using System.IO;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DiskLoadingViewModel : IDiskLoadingViewModel
    {
        private readonly DiskContentsViewModel _contentsModel;

        private readonly DiskSelectionViewModel _selectionModel;

        private readonly IEventLogViewModel _eventLogModel;

        private bool _isFat16Checked;

        private VolumeLocker _volumeLocker;

        private Stream _diskStream;

        private IFileSystem _fileSystem;

        public DiskLoadingViewModel(
            DiskContentsViewModel contentsModel,
            DiskSelectionViewModel selectionModel,
            IEventLogViewModel eventLogModel)
        {
            _contentsModel = contentsModel;
            _selectionModel = selectionModel;
            _eventLogModel = eventLogModel;

            _selectionModel.PropertyChanged += selectionModel_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEnabled => _selectionModel.SelectedDisk != null;

        public bool IsLoadEnabled => !IsCloseEnabled;

        public bool IsCloseEnabled { get; private set; }

        public bool IsFatSelectorsEnabled => !IsCloseEnabled;

        public bool IsFat16Checked
        {
            get { return _isFat16Checked; }
            set
            {
                _isFat16Checked = value;

                PropertyChanged.OnPropertyChanged(() => IsFat16Checked);
            }
        }

        public bool IsFat32Checked
        {
            get { return !_isFat16Checked; }
            set
            {
                _isFat16Checked = !value;

                PropertyChanged.OnPropertyChanged(() => IsFat32Checked);
            }
        }

        public void Init()
        {
            IsFat16Checked = true;
        }

        public void Load()
        {
            _eventLogModel.AppendEvent("Trying to load the disk...");

            try
            {
                _volumeLocker = VolumeLocker.LockAndDismount(_selectionModel.SelectedDisk);

                var driveNumber = VolumeToDriveNumber.Map(_selectionModel.SelectedDisk);

                var layoutInfo = PhysicalDriveManager.GetDriveLayoutInformation(driveNumber);

                DriveGeometry driveGeometry;
                _diskStream = IO.Streams.PhysicalDriveStream.OpenBuffered(driveNumber, out driveGeometry);

                _diskStream.Seek(layoutInfo.PartitionEntries[0].StartingOffset, SeekOrigin.Begin);

                _fileSystem =
                    _isFat16Checked
                        ? new FileSystemFat16(_diskStream)
                        : (FileSystemFatBase) new FileSystemFat32(_diskStream);

                IsCloseEnabled = true;

                PropertyChanged.OnPropertyChanged(() => IsLoadEnabled);
                PropertyChanged.OnPropertyChanged(() => IsCloseEnabled);
                PropertyChanged.OnPropertyChanged(() => IsFatSelectorsEnabled);

                _eventLogModel.AppendEvent("Success");

                _contentsModel.FileSystem = _fileSystem;
                _selectionModel.AreContentsLoaded = true;
            }
            catch (Exception e)
            {
                _eventLogModel.AppendEvent("Failure: " + e.Message);

                if (_diskStream != null)
                {
                    _diskStream.Dispose();
                    _diskStream = null;
                }

                if (_volumeLocker != null)
                {
                    _volumeLocker.Dispose();
                    _volumeLocker = null;
                }
            }
        }

        public void Close()
        {
            try
            {
                _eventLogModel.AppendEvent("Trying to close the disk...");

                _fileSystem.Flush();
                _fileSystem.Dispose();
                _fileSystem = null;

                _diskStream.Dispose();
                _diskStream = null;

                _volumeLocker.Dispose();
                _volumeLocker = null;

                IsCloseEnabled = false;

                PropertyChanged.OnPropertyChanged(() => IsLoadEnabled);
                PropertyChanged.OnPropertyChanged(() => IsCloseEnabled);
                PropertyChanged.OnPropertyChanged(() => IsFatSelectorsEnabled);

                _eventLogModel.AppendEvent("Success");

                _contentsModel.FileSystem = null;
                _selectionModel.AreContentsLoaded = false;
            }
            catch (Exception e)
            {
                _eventLogModel.AppendEvent("Failure: " + e.Message);
            }
        }

        private void selectionModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.ByOrdinal("SelectedDisk"))
            {
                PropertyChanged.OnPropertyChanged(() => IsEnabled);
            }
        }
    }
}