// -----------------------------------------------------------------------
// <copyright file="IDiskContentsViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDiskContentsViewModel : INotifyPropertyChanged
    {
        bool IsEnabled { get; }

        IDirectoryViewModel RootDirectory { get; }

        IFileSystemEntityViewModel SelectedEntity { get; set; }

        bool IsButtonProtectEnabled { get; }

        bool IsButtonUnprotectEnabled { get; }

        void Protect();

        void Unprotect();
    }
}