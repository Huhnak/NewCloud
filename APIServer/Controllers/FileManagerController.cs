using APIServer.Authorization;
using APIServer.Helpers;
using APIServer.Models;
using APIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace APIServer.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class FileManagerController : ControllerBase
{
    private IFileService _fileService;

    public FileManagerController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    [DisableRequestSizeLimit]
    [Authorize]
    public IActionResult UploadFile([FromForm] FilesForm filesForm)
    {
        _fileService.UploadFiles(filesForm, HttpContext);
        return Ok();
    }

    [HttpPost("deleteFile")]
    [DisableRequestSizeLimit]
    [Authorize]
    public IActionResult DeleteFile([FromForm] FileForm fileForm)
    {
        _fileService.DeleteFile(fileForm, HttpContext);
        return Ok();
    }

    [HttpPost("deleteDirectoryRecursively")]
    [DisableRequestSizeLimit]
    [Authorize]
    public IActionResult DeleteDirectoryRecursively([FromForm] DirectoryDelete directoryDelete)
    {
        _fileService.DeleteDirectoryRecursively(directoryDelete, HttpContext);
        return Ok();
    }
    [HttpPost("deleteDirectoryNotRecursively")]
    [DisableRequestSizeLimit]
    [Authorize]
    public IActionResult DeleteDirectoryNotRecursively([FromForm] DirectoryDelete directoryDelete)
    {
        _fileService.DeleteDirectoryNotRecursively(directoryDelete, HttpContext);
        return Ok();
    }
    [HttpPost("createDirectory")]
    [DisableRequestSizeLimit]
    [Authorize]
    public IActionResult CreateDirectory([FromForm] DirectoryPathModel directoryCreate)
    {
        _fileService.CreateDirectory(directoryCreate, HttpContext);
        return Ok();
    }
    [HttpGet("getDirectoriesTree")]
    [DisableRequestSizeLimit]
    [Authorize]
    public IActionResult GetDirectoriesTree()
    {
        DirectoryTree directoriesTree = _fileService.GetDirectoriesTree(HttpContext);
        var json = JsonSerializer.Serialize(directoriesTree);
        return Ok(json);
    }
    [HttpPost("getFileByDirectory")]
    [DisableRequestSizeLimit]
    [Authorize]
    public IActionResult GetFileByDirectory([FromForm] FileForm fileForm)
    {
        var formFile = _fileService.GetFileByDirectory(fileForm, HttpContext);
        return formFile;
    }
}
