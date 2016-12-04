namespace Metran.FileSystem
{
    /// <summary>
    /// Defines properties that are specific to directories
    /// </summary>
    public interface IDirectory : IFileSystemEntity
    {
        bool IsRoot { get; }

        IDirectory CreateSubdirectory(string name);

        IFile CreateFile(string name);

        IFileSystemEntity[] GetFileSystemEntities();

        IDirectory[] GetDirectories();

        IFile[] GetFiles();

        void DeleteRecursive();
    }
}