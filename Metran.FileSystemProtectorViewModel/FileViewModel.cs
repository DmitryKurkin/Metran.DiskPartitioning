// -----------------------------------------------------------------------
// <copyright file="FileViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Metran.FileSystem;
using System;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class FileViewModel : FileSystemEntityViewModel, IFileViewModel
    {
        public FileViewModel(IFile file)
            : base(file)
        {
        }

        public long Length
        {
            get
            {
                var file = Entity as IFile;
                if (file == null)
                {
                    throw new InvalidOperationException("The entity is not a file");
                }

                return file.Length;
            }
        }
    }
}