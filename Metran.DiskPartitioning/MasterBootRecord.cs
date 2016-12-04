using System;
using System.IO;

namespace Metran.DiskPartitioning
{
    /// <summary>
    /// Represents a partition table implementing the conventional IBM PC partitioning scheme
    /// </summary>
    public class MasterBootRecord
    {
        public const ushort ValidNulls = 0x0000;

        public const ushort ValidMbrSignature = 0xAA55;

        public const int CodeAreaLength = 440;

        public const int PrimaryPartitionsCount = 4;

        private readonly byte[] _codeArea;

        private readonly PartitionRecord[] _primaryPartitions;

        public MasterBootRecord(uint diskSignature)
        {
            DiskSignature = diskSignature;

            Nulls = ValidNulls;
            MbrSignature = ValidMbrSignature;

            _codeArea = new byte[CodeAreaLength];

            _primaryPartitions = new PartitionRecord[PrimaryPartitionsCount];
            for (var i = 0; i < _primaryPartitions.Length; i++)
            {
                _primaryPartitions[i] = new PartitionRecord();
            }
        }

        public MasterBootRecord()
            : this(0)
        {
        }

        public MasterBootRecord(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            _codeArea = binReader.ReadBytes(CodeAreaLength);
            DiskSignature = binReader.ReadUInt32();
            Nulls = binReader.ReadUInt16();

            _primaryPartitions = new PartitionRecord[PrimaryPartitionsCount];
            for (var i = 0; i < _primaryPartitions.Length; i++)
            {
                _primaryPartitions[i] = new PartitionRecord(input);
            }

            MbrSignature = binReader.ReadUInt16();
        }

        public byte[] CodeArea
        {
            get
            {
                var codeAreaCopy = new byte[_codeArea.Length];
                _codeArea.CopyTo(codeAreaCopy, 0);

                return codeAreaCopy;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length != CodeAreaLength)
                {
                    throw new ArgumentException(
                        $"The code area length ({value.Length}) is invalid. The expected value is {CodeAreaLength}",
                        nameof(value));
                }

                value.CopyTo(_codeArea, 0);
            }
        }

        public uint DiskSignature { get; set; }

        public ushort Nulls { get; set; }

        public PartitionRecord[] PrimaryPartitions
        {
            get
            {
                var primaryPartitionsCopy = new PartitionRecord[_primaryPartitions.Length];
                _primaryPartitions.CopyTo(primaryPartitionsCopy, 0);

                return primaryPartitionsCopy;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                if (value.Length != PrimaryPartitionsCount)
                {
                    throw new ArgumentException(
                        $"The primary partitions number ({value.Length}) is invalid. The expected value is {PrimaryPartitionsCount}",
                        nameof(value));
                }

                value.CopyTo(_primaryPartitions, 0);
            }
        }

        public ushort MbrSignature { get; set; }

        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(_codeArea);
            binWriter.Write(DiskSignature);
            binWriter.Write(Nulls);

            for (var i = 0; i < _primaryPartitions.Length; i++)
            {
                _primaryPartitions[i].Save(output);
            }

            binWriter.Write(MbrSignature);
        }
    }
}