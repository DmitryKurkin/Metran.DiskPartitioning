// -----------------------------------------------------------------------
// <copyright file="DirectoryViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Metran.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DirectoryViewModel : FileSystemEntityViewModel, IDirectoryViewModel
    {
        public DirectoryViewModel(IDirectory directory)
            : base(directory)
        {
        }

        public bool IsRoot
        {
            get
            {
                var dir = Entity as IDirectory;
                if (dir == null)
                {
                    throw new InvalidOperationException("The entity is not a directory");
                }

                return dir.IsRoot;
            }
        }

        public IEnumerable<IFileViewModel> Files
        {
            get
            {
                var dir = Entity as IDirectory;
                if (dir == null)
                {
                    throw new InvalidOperationException("The entity is not a directory");
                }

                return dir.GetFiles().Select(f => new FileViewModel(f));
            }
        }

        public IEnumerable<IDirectoryViewModel> Directories
        {
            get
            {
                var dir = Entity as IDirectory;
                if (dir == null)
                {
                    throw new InvalidOperationException("The entity is not a directory");
                }

                return dir.GetDirectories().Select(d => new DirectoryViewModel(d));
            }
        }
    }
}