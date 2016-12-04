// -----------------------------------------------------------------------
// <copyright file="FileSystemEntityViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Metran.FileSystem;
using System;
using System.ComponentModel;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class FileSystemEntityViewModel : IFileSystemEntityViewModel
    {
        protected FileSystemEntityViewModel(IFileSystemEntity entity)
        {
            Entity = entity;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => Entity.Name;

        public bool IsNormal
            => (Entity.Attributes & FileSystemEntityAttributes.Normal) == FileSystemEntityAttributes.Normal;

        public bool IsReadOnly
            => (Entity.Attributes & FileSystemEntityAttributes.ReadOnly) == FileSystemEntityAttributes.ReadOnly;

        public bool IsHidden
            => (Entity.Attributes & FileSystemEntityAttributes.Hidden) == FileSystemEntityAttributes.Hidden;

        public bool IsSystem
            => (Entity.Attributes & FileSystemEntityAttributes.System) == FileSystemEntityAttributes.System;

        public bool IsArchive
            => (Entity.Attributes & FileSystemEntityAttributes.Archive) == FileSystemEntityAttributes.Archive;

        public DateTime CreationDate => Entity.CreationDate;

        public DateTime LastAccessDate => Entity.LastAccessDate;

        public DateTime LastWriteDate => Entity.LastWriteDate;

        public bool Exists => Entity.Exists;

        protected IFileSystemEntity Entity { get; }

        public void Rename(string newName)
        {
            Entity.Rename(newName);
        }
    }
}