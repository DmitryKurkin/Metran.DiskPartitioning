// -----------------------------------------------------------------------
// <copyright file="DiskContentsViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Gets.Utils;
using Metran.FileSystem;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DiskContentsViewModel : IDiskContentsViewModel
    {
        private const char ProtectChar = '>';

        private static bool IsProtected(IFileSystemEntityViewModel entityModel)
        {
            return entityModel.Name.StartsWith(
                ProtectChar.ToString(CultureInfo.CurrentCulture),
                StringComparison.OrdinalIgnoreCase);
        }

        private IFileSystem _fileSystem;

        private IFileSystemEntityViewModel _selectedEntity;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEnabled => _fileSystem != null;

        public IDirectoryViewModel RootDirectory => new DirectoryViewModel(_fileSystem.RootDirectory);

        public IFileSystemEntityViewModel SelectedEntity
        {
            get { return _selectedEntity; }

            set
            {
                _selectedEntity = value;

                PropertyChanged.OnPropertyChanged(() => SelectedEntity);
                PropertyChanged.OnPropertyChanged(() => IsButtonProtectEnabled);
                PropertyChanged.OnPropertyChanged(() => IsButtonUnprotectEnabled);
            }
        }

        public bool IsButtonProtectEnabled => _selectedEntity != null && !IsProtected(_selectedEntity);

        public bool IsButtonUnprotectEnabled => _selectedEntity != null && IsProtected(_selectedEntity);

        public IFileSystem FileSystem
        {
            get { return _fileSystem; }

            set
            {
                _fileSystem = value;

                PropertyChanged.OnPropertyChanged(() => IsEnabled);
                PropertyChanged.OnPropertyChanged(() => RootDirectory);
                PropertyChanged.OnPropertyChanged(() => SelectedEntity);
                PropertyChanged.OnPropertyChanged(() => IsButtonProtectEnabled);
                PropertyChanged.OnPropertyChanged(() => IsButtonUnprotectEnabled);
            }
        }

        public void Protect()
        {
            var currName = _selectedEntity.Name;

            _selectedEntity.Rename(ProtectChar + currName);

            PropertyChanged.OnPropertyChanged(() => IsButtonProtectEnabled);
            PropertyChanged.OnPropertyChanged(() => IsButtonUnprotectEnabled);
        }

        public void Unprotect()
        {
            var currName = _selectedEntity.Name;

            _selectedEntity.Rename(currName.TrimStart(ProtectChar));

            PropertyChanged.OnPropertyChanged(() => IsButtonProtectEnabled);
            PropertyChanged.OnPropertyChanged(() => IsButtonUnprotectEnabled);
        }
    }
}