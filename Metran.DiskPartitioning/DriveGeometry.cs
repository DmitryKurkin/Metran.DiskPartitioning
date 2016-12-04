namespace Metran.DiskPartitioning
{
    public class DriveGeometry
    {
        public DriveGeometry(
            int mediaType,
            long size,
            int bytesPerSector,
            long cylinders,
            int sectorsPerTrack,
            int tracksPerCylinder)
        {
            MediaType = mediaType;
            Size = size;
            BytesPerSector = bytesPerSector;
            Cylinders = cylinders;
            SectorsPerTrack = sectorsPerTrack;
            TracksPerCylinder = tracksPerCylinder;
        }

        public int MediaType { get; }

        public long Size { get; }

        public int BytesPerSector { get; }

        public long Cylinders { get; }

        public int SectorsPerTrack { get; }

        public int TracksPerCylinder { get; }
    }
}