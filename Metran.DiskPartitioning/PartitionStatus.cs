namespace Metran.DiskPartitioning
{
    public enum PartitionStatus : byte
    {
        NonBootable = 0x00,
        Bootable = 0x80
    }
}