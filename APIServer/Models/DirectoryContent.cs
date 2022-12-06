namespace APIServer.Models;

public class DirectoryContent
{
    public List<IFormFile> Files { get; set; }
    public List<DirectoryInfo> DirectoryInfos { get; set; }
}
