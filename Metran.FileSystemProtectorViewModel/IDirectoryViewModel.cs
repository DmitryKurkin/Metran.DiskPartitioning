// -----------------------------------------------------------------------
// <copyright file="IDirectoryViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDirectoryViewModel : IFileSystemEntityViewModel
    {
        bool IsRoot { get; }

        IEnumerable<IFileViewModel> Files { get; }

        IEnumerable<IDirectoryViewModel> Directories { get; }
    }
}