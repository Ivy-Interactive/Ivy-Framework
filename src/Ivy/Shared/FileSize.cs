namespace Ivy.Shared;

public static class FileSize
{
    public static long FromBytes(double bytes)
    {
        return (long)bytes;
    }

    public static long FromKilobytes(double kilobytes)
    {
        return (long)(kilobytes * 1024);
    }

    public static long FromMegabytes(double megabytes)
    {
        return (long)(megabytes * 1024 * 1024);
    }

    public static long FromGigabytes(double gigabytes)
    {
        return (long)(gigabytes * 1024 * 1024 * 1024);
    }

    public static long FromTerabytes(double terabytes)
    {
        return (long)(terabytes * 1024 * 1024 * 1024 * 1024);
    }
}