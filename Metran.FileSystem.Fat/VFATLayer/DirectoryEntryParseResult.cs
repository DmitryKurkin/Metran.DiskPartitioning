namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Specifies the result of directory entry parsing
    /// </summary>
    public enum DirectoryEntryParseResult
    {
        InvalidEntry,
        LastEntry,
        FreeEntry,
        ShortEntry,
        LongEntry,
        VolumeLabelEntry
    }
}