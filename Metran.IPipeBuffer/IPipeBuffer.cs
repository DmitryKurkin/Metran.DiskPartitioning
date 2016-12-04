namespace Metran.IO.Streams
{
    /// <summary>
    /// Represents a buffer of bytes that acts as a pipe.
    /// Used as a convenient way to buffer I/O for streams that support only fixed-size block operations
    /// </summary>
    public interface IPipeBuffer
    {
        int BytesAvailable { get; }

        long TotalBytesRead { get; }

        long TotalBytesFed { get; }

        void Read(byte[] buffer, int offset, int count);

        void Read(byte[] buffer);

        int Feed(byte[] buffer, int offset, int count);

        int Feed(byte[] buffer);

        void Reset();
    }
}