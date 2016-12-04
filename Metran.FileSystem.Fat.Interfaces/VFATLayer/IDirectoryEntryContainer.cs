using System.Collections.ObjectModel;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Represents a logical container of directory entries
    /// </summary>
    public interface IDirectoryEntryContainer
    {
        ReadOnlyCollection<IDirectoryEntry> Entries { get; }
    }
}