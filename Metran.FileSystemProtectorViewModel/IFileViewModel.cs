// -----------------------------------------------------------------------
// <copyright file="IFileViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IFileViewModel : IFileSystemEntityViewModel
    {
        long Length { get; }
    }
}