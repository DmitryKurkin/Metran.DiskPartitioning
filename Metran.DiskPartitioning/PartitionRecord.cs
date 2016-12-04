using System;
using System.IO;

namespace Metran.DiskPartitioning
{
    /// <summary>
    /// Represents a record in an IBM partition table
    /// </summary>
    public class PartitionRecord
    {
        private ChsAddress _firstSectorChs;

        private ChsAddress _lastSectorChs;

        public PartitionRecord()
        {
            _firstSectorChs = new ChsAddress();
            _lastSectorChs = new ChsAddress();
        }

        public PartitionRecord(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            Status = binReader.ReadByte();
            _firstSectorChs = new ChsAddress(input);
            PartitionType = binReader.ReadByte();
            _lastSectorChs = new ChsAddress(input);
            FirstSectorLba = binReader.ReadUInt32();
            SectorsCount = binReader.ReadUInt32();
        }

        public byte Status { get; set; }

        public ChsAddress FirstSectorChs
        {
            get { return _firstSectorChs; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _firstSectorChs = value;
            }
        }

        public byte PartitionType { get; set; }

        public ChsAddress LastSectorChs
        {
            get { return _lastSectorChs; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _lastSectorChs = value;
            }
        }

        public uint FirstSectorLba { get; set; }

        public uint SectorsCount { get; set; }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(Status);
            _firstSectorChs.Save(output);
            binWriter.Write(PartitionType);
            _lastSectorChs.Save(output);
            binWriter.Write(FirstSectorLba);
            binWriter.Write(SectorsCount);
        }
    }
}