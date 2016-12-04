// -----------------------------------------------------------------------
// <copyright file="DriveGeometry.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

namespace Metran.IO.Streams
{
    /// <summary>
    /// Defines a set of parameters that describe the geometry of a physical drive
    /// </summary>
    public class DriveGeometry
    {
        internal DriveGeometry(
            long size,
            int mediaType,
            int bytesPerSector,
            long cylinders,
            int sectorsPerTrack,
            int tracksPerCylinder)
        {
            Size = size;
            MediaType = mediaType;
            BytesPerSector = bytesPerSector;
            Cylinders = cylinders;
            SectorsPerTrack = sectorsPerTrack;
            TracksPerCylinder = tracksPerCylinder;
        }

        public long Size { get; }

        public int MediaType { get; }

        public int BytesPerSector { get; }

        public long Cylinders { get; }

        public int SectorsPerTrack { get; }

        public int TracksPerCylinder { get; }
    }
}