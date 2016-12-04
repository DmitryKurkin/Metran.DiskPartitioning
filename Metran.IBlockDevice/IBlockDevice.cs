namespace Metran.IO.Streams
{
    // TODO: IDisposable

    /// <summary>
    /// Represents a device that reads and writes data by fixed-size blocks. Returns zero bytes if all data has been read
    /// </summary>
    public interface IBlockDevice
    {
        int BlockSize { get; }

        bool SupportsPositioning { get; }

        bool SupportsReading { get; }

        bool SupportsWriting { get; }

        long Position(long value);

        byte[] ReadBlock();

        void WriteBlock(byte[] blockData);

        void Close();
    }
}