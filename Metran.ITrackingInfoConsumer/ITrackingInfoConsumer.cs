namespace Metran.IO.Streams
{
    /// <summary>
    /// Defines a method allowing an IOTrackingStream to assign a client the total number of bytes that were read from and written to the stream upon closing
    /// </summary>
    public interface ITrackingInfoConsumer
    {
        void AssignTrackingInfo(long totalBytesRead, long totalBytesWritten);
    }
}