namespace APIServer.Helpers;

public static class BytesConverter
{
    public static float Megabyte(float megabyte)
    {
        return megabyte * 1024;
    }
    public static float Gigabyte(float gigabyte)
    {
        return gigabyte * 1024 * 1024;
    }

}
