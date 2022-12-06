namespace APIServer.Models;

public class FilesForm
{
    public List<IFormFile> Files { get; set; }
    public string Path { get; set; }
}
