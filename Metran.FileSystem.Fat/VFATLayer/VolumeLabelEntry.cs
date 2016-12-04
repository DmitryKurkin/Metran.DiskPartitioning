using System;
using System.IO;
using System.Text;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Represents a directory entry that holds the label of a volume
    /// </summary>
    public class VolumeLabelEntry : IDirectoryEntry
    {
        public const int MaxLabelLength = 11;

        private const int UnusedBytesLength = 20;

        private readonly DirectoryEntryAttributes _attributes;

        private readonly byte[] _unusedBytes;

        private byte[] _labelBytes;

        private string GetLabel()
        {
            var label = Encoding.ASCII.GetString(_labelBytes);

            label = label.TrimEnd(' ');

            return label;
        }

        private void SetLabel(string label)
        {
            // don't forget to pad it with spaces...

            var spacePaddedLabel = label.PadRight(MaxLabelLength);

            _labelBytes = Encoding.ASCII.GetBytes(spacePaddedLabel);
        }

        string IDirectoryEntry.Name => Label;

        DirectoryEntryAttributes IDirectoryEntry.EntryAttributes
        {
            get { return _attributes; }
            set { throw new NotSupportedException(); }
        }

        DateTime IDirectoryEntry.CreationDate
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        DateTime IDirectoryEntry.LastAccessDate
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        DateTime IDirectoryEntry.LastWriteDate
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        int IDirectoryEntry.FirstCluster
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        uint IDirectoryEntry.Size
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        void IDirectoryEntry.Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.CanWrite)
            {
                throw new ArgumentException("The output stream must be writeable", nameof(output));
            }

            var binWriter = new BinaryWriter(output);

            binWriter.Write(_labelBytes);
            binWriter.Write((byte) _attributes);
            binWriter.Write(_unusedBytes);
        }

        public VolumeLabelEntry()
        {
            _labelBytes = new byte[MaxLabelLength];
            _unusedBytes = new byte[UnusedBytesLength];

            _attributes = DirectoryEntryAttributes.VolumeLabel;
        }

        public VolumeLabelEntry(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            var binReader = new BinaryReader(input);

            _labelBytes = binReader.ReadBytes(MaxLabelLength);
            _attributes = (DirectoryEntryAttributes) binReader.ReadByte();
            _unusedBytes = binReader.ReadBytes(UnusedBytesLength);
        }

        internal string Label
        {
            get { return GetLabel(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The label must not be empty", nameof(value));
                }

                if (value.Length > MaxLabelLength)
                {
                    throw new ArgumentException(
                        $"The label length ({value.Length}) is more than the maximum allowable value ({MaxLabelLength})",
                        nameof(value));
                }

                SetLabel(value);
            }
        }
    }
}