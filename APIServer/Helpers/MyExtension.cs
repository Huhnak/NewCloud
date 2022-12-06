using System.Text.RegularExpressions;

namespace APIServer.Helpers;

public static class MyExtension
{
    public static string GetLocalPath(this FileInfo file, AppSettings appSettings)
    {
        string localPath = file.DirectoryName.Replace("\\", "/");
        string cloudAbsolutePath = appSettings.CloudAbsolutePath.Replace("\\", "/");
        Regex regex = new Regex($@"^({cloudAbsolutePath}\d+/)");
        localPath = regex.Replace(localPath, "");
        localPath += "/";
        return localPath;
    }
}
