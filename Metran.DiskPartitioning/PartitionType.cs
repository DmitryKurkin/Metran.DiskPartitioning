using System.Diagnostics.CodeAnalysis;

namespace Metran.DiskPartitioning
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum PartitionType : byte
    {
        EmptyPartition = 0x0,
        FAT12 = 0x01,
        XENIXroot = 0x02,
        XENIXusr = 0x03,
        FAT16Less32MiB = 0x04,
        ExtendedPartition = 0x05,
        FAT16Greater32MiB = 0x06,
        NTFS = 0x07,
        AIX = 0x08,
        AIXBootable = 0x09,
        OS2BootManager = 0x0A,
        Windows95FAT32 = 0x0B,
        Windows95FAT32LBA = 0x0C,
        Windows95FAT16LBA = 0x0E,
        ExtendedPartitionLBA = 0x0F,
        OPUS = 0x10,
        HiddenFAT12 = 0x11,
        CompaqDiagnosticsPartition = 0x12,
        HiddenFAT16 = 0x14,
        HiddenNTFS = 0x17,
        HiddenFAT32 = 0x1B,
        HiddenFAT32LBA = 0x1C,
        HiddenFAT16LBA = 0x1D,
        XOSLBootloaderFilesystem = 0x78,
        LinuxSwapSpace = 0x82,
        NativeLinuxFileSystem = 0x83,
        LinuxExtended = 0x85,
        LegacyFTFAT16 = 0x86,
        LegacyFTNTFS = 0x87,
        LinuxPlaintext = 0x88,
        GnuLinuxLVM = 0x89,
        LegacyFTFAT32 = 0x8B,
        LegacyFTFAT32LBA = 0x8C,
        LinuxLVM = 0x8E,
        BSDSlice = 0xA5,
        RawData = 0xDA,
        BootIt = 0xDF,
        BFS = 0xEB,
        EFI = 0xEF,
        VMwareVMFS = 0xFB,
        VMwareVMKCORE = 0xFC,
        LinuxRAIDAuto = 0xFD
    }
}