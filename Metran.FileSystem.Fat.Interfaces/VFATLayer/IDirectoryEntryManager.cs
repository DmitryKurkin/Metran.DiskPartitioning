using System.IO;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Represents an entry point of the VFAT subsystem. Forces the VFAT naming conventions for directory entries
    /// </summary>
    public interface IDirectoryEntryManager
    {
        IDirectoryEntry CreateDotEntry();

        IDirectoryEntry CreateDotdotEntry();

        IDirectoryEntry CreateVolumeLabelEntry();

        IDirectoryEntry CreateRootDirectoryEntry();

        IDirectoryEntry CreateEntry(IDirectoryEntryContainer targetContainer, string name);

        void RenameEntry(IDirectoryEntry entry, IDirectoryEntryContainer targetContainer, string newName);

        void RenameVolumeLabelEntry(IDirectoryEntry volumeLabelEntry, string newName);

        bool MatchEntryName(IDirectoryEntry entry, string name);

        IDirectoryEntry[] LoadEntries(Stream input, bool returnErroneousEntries);

        int GetSize(IDirectoryEntry entry);
    }
}