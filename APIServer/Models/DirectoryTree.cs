using APIServer.Entities;

namespace APIServer.Models;

public class DirectoryTree
{
    public List<DBFile> Files { get; set; }
    public List<string> Directories { get; set; }
}
