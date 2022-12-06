using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIServer.Entities;

public class DBFile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public int Size { get; set; }
    public string serverIP { get; set; }
    public string LocalPath { get; set; }

}
