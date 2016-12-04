// -----------------------------------------------------------------------
// <copyright file="IFileSystemEntityViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IFileSystemEntityViewModel : INotifyPropertyChanged
    {
        string Name { get; }

        bool IsNormal { get; }

        bool IsReadOnly { get; }

        bool IsHidden { get; }

        bool IsSystem { get; }

        bool IsArchive { get; }

        DateTime CreationDate { get; }

        DateTime LastAccessDate { get; }

        DateTime LastWriteDate { get; }

        bool Exists { get; }

        void Rename(string newName);
    }
}