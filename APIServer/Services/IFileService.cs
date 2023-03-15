using APIServer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace APIServer.Services;

public interface IFileService
{
    void UploadFiles(FilesForm filesForm, HttpContext httpContext);
    void DeleteFile(FileForm fileForm, HttpContext httpContext);
    void DeleteDirectoryNotRecursively(DirectoryDelete directoryDelete, HttpContext httpContext);
    void DeleteDirectoryRecursively(DirectoryDelete directoryDelete, HttpContext httpContext);
    void CreateDirectory(DirectoryPathModel directoryCreate, HttpContext httpContext);
    string GetDirectoriesTree(HttpContext httpContext);
    FileStreamResult GetFileByDirectory(FileForm fileForm, HttpContext httpContext);
}