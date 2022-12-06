using APIServer.Helpers;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http;
using System.Text;
using System.Web;
using APIServer.Entities;
using System.Xml.Linq;
using System.Drawing;
using Org.BouncyCastle.Bcpg;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net;
using APIServer.Models;
using APIServer.Helpers;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.WebUtilities;

namespace APIServer.Services;

public class FileService : IFileService
{
    private readonly DataContext _context;
    private readonly AppSettings _appSettings;
    public FileService(DataContext context, IOptions<AppSettings> appSettings)
    {
        _context = context;
        _appSettings = appSettings.Value;
    }
    public void UploadFiles(FilesForm filesForm, HttpContext httpContext)
    {
        checkDirectoryCorrectness(filesForm.Path);
        foreach (IFormFile file in filesForm.Files)
        {
            checkFileNameCorrectness(file.FileName);
        }

        User currentUser = (User)httpContext.Items["User"];
        string userPath = Path.Combine(_appSettings.CloudAbsolutePath, currentUser.Id.ToString());
        string dirPath = Path.Combine(userPath, filesForm.Path);

        // Есть ли такая папка
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        // Есть ли такие файлы, если есть убрать их из спика файлов
        var tempFilesForm = new List<IFormFile>(filesForm.Files);
        foreach (IFormFile file in tempFilesForm)
        {
            if (File.Exists(dirPath + file.FileName))
            {
                filesForm.Files.Remove(file);
                // Если всех фалов из списка нет, то выдать исключение
                if (filesForm.Files.Count == 0)
                    throw new AppException("Files with the same name exist in this directory");
            }
        }

        long spaceLeft = getSpaceLeft(currentUser);
        long tempSpaceLeft = spaceLeft;
        foreach (IFormFile fl in filesForm.Files)
        {
            tempSpaceLeft-=fl.Length;
            if (tempSpaceLeft < 0)
            {
                throw new AppException($"No space left, you have: {spaceLeft} bytes");
            }
        }

        List<DBFile> dbFiles = new List<DBFile>();
        foreach ( IFormFile fl in filesForm.Files)
        {
            string filePath = Path.Combine(dirPath, fl.FileName);
            using (Stream stream = new FileStream(filePath, FileMode.Create))
            {
                fl.CopyTo(stream);
                dbFiles.Add(new DBFile()
                {
                    UserId = currentUser.Id,
                    Name = fl.FileName,
                    Size = (int)fl.Length,
                    LocalPath = filesForm.Path,
                    serverIP = _appSettings.PublicIpAddress,
                });
            }
        }
        int summaryFilesSize = 0;
        filesForm.Files.ForEach(f => summaryFilesSize += (int)f.Length);
        decreaseSpaceLeft(currentUser, summaryFilesSize);

        _context.DBFile.AddRange(dbFiles);
        _context.SaveChanges();
        
    }
    public void DeleteFile(FileForm fileForm, HttpContext httpContext)
    {
        checkFileNameCorrectness(fileForm.FileName);
        checkDirectoryCorrectness(fileForm.DirectoryPath);

        User currentUser = (User)httpContext.Items["User"];
        string userPath = Path.Combine(_appSettings.CloudAbsolutePath, currentUser.Id.ToString());
        string dirPath = Path.Combine(userPath, fileForm.DirectoryPath);
        string fileFullPath = Path.Combine(dirPath, fileForm.FileName);

        if (!Directory.Exists(dirPath))
            throw new AppException("This dirrectory is not exist");
        if (!File.Exists(fileFullPath))
            throw new AppException($"This file in not exist in {fileForm.DirectoryPath} directory");

        FileInfo targetFile = new FileInfo(fileFullPath);
        try
        {
            var fileToDelete = _context.DBFile.Where(f => f.LocalPath == fileForm.DirectoryPath && f.Name == fileForm.FileName).First();
            _context.DBFile.Remove(fileToDelete);
        }
        catch(ArgumentNullException ex)
        {
            throw new AppException($"No such file in this directory | {ex.Message}");
        }
        

        increaseSpaceLeft(currentUser, (int)targetFile.Length);
        File.Delete(fileFullPath);

        _context.SaveChanges();
    }
    public void DeleteDirectoryNotRecursively(DirectoryDelete directoryDelete, HttpContext httpContext)
    {
        checkDirectoryCorrectness(directoryDelete.DirectoryPath);

        User currentUser = (User)httpContext.Items["User"];
        string userPath = Path.Combine(_appSettings.CloudAbsolutePath, currentUser.Id.ToString());
        string dirPath = Path.Combine(userPath, directoryDelete.DirectoryPath);

        if (!Directory.Exists(dirPath))
            throw new AppException("Directory no longer exists");
        try
        {
            Directory.Delete(dirPath, false); 
        }
        catch(IOException ex)
        {
            throw new AppException("Directory consists of files or subdirectories");
        }
    }
    public void DeleteDirectoryRecursively(DirectoryDelete directoryDelete, HttpContext httpContext)
    {
        checkDirectoryCorrectness(directoryDelete.DirectoryPath);

        User currentUser = (User)httpContext.Items["User"];
        DirectoryInfo userDirectoryAbsolute = new DirectoryInfo(Path.Combine(_appSettings.CloudAbsolutePath, currentUser.Id.ToString()));
        DirectoryInfo directoryAbsolute = new DirectoryInfo(Path.Combine(userDirectoryAbsolute.FullName, directoryDelete.DirectoryPath));
        
        if (!directoryAbsolute.Exists)
            throw new AppException("Directory no longer exists");
        var subDirectories = directoryAbsolute.GetDirectories("*", SearchOption.AllDirectories);
        subDirectories = subDirectories.Prepend(directoryAbsolute).ToArray();
        int summaryFilesSize = 0;
        foreach (DirectoryInfo dir in subDirectories.Reverse())
        {
            var files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                var sdfgd = file.GetLocalPath(_appSettings);
                var filesToRemove = _context.DBFile.Where(f => f.Name == file.Name && f.LocalPath == file.GetLocalPath(_appSettings) && f.UserId == currentUser.Id);
                
                foreach (var fileToRemove in filesToRemove)
                {
                    summaryFilesSize += fileToRemove.Size;
                }
                _context.DBFile.RemoveRange(filesToRemove);
            }
        }
        directoryAbsolute.Delete(true);
        increaseSpaceLeft(currentUser, summaryFilesSize);
        _context.SaveChanges();

    }
    public void CreateDirectory(DirectoryPathModel directoryCreate, HttpContext httpContext)
    {
        checkDirectoryCorrectness(directoryCreate.DirectoryPath);

        User currentUser = (User)httpContext.Items["User"];
        string userPath = Path.Combine(_appSettings.CloudAbsolutePath, currentUser.Id.ToString());
        string dirPath = Path.Combine(userPath, directoryCreate.DirectoryPath);

        if (Directory.Exists(dirPath))
            throw new AppException("Directory already exist");
        Directory.CreateDirectory(dirPath);
    }
    public DirectoryTree GetDirectoriesTree(HttpContext httpContext)
    {
        User currentUser = (User)httpContext.Items["User"];
        DirectoryInfo userDirectory = new DirectoryInfo(Path.Combine(_appSettings.CloudAbsolutePath, currentUser.Id.ToString()));

        DirectoryTree directoryTree = new DirectoryTree();

        directoryTree.Files = _context.DBFile.Where(f => f.UserId == currentUser.Id).ToList();
        directoryTree.Directories = new List<string>();
        //var alreadyExistsDirectories = directoryTree.Files.Select(f => f.LocalPath).Distinct().ToList();
        foreach (var item in userDirectory.GetDirectories("*", SearchOption.AllDirectories))
        {
            var tmpPath = Path.GetRelativePath(userDirectory.FullName, item.FullName).Replace("\\","/")+"/";
            directoryTree.Directories.Add(tmpPath);
        }
        return directoryTree; 
    }
    public FileStreamResult GetFileByDirectory(FileForm fileForm, HttpContext httpContext)
    {
        checkDirectoryCorrectness(fileForm.DirectoryPath);
        checkFileNameCorrectness(fileForm.FileName);
        User currentUser = (User)httpContext.Items["User"];
        string userPath = Path.Combine(_appSettings.CloudAbsolutePath, currentUser.Id.ToString());
        string directoryPath = Path.Combine(userPath, fileForm.DirectoryPath);
        FileInfo fileInfo = new FileInfo(Path.Combine(directoryPath, fileForm.FileName));

        Stream stream = new FileStream(fileInfo.FullName, FileMode.Open);
        FileStreamResult fileStreamResult = new FileStreamResult(stream, "multipart/form-data");
        return fileStreamResult;
    }
    private long getSpaceLeft(User user)
    {
        long spaceLeft = _context.User.First(u => u.Id == user.Id).SpaceLeft;
        return spaceLeft;
        //return BytesConverter.Megabyte(50);
    }
    private void increaseSpaceLeft(User user, int size)
    {
        _context.User.First(u => u.Id == user.Id).SpaceLeft += size;
    }
    private void decreaseSpaceLeft(User user, int size)
    {
        _context.User.First(u => u.Id == user.Id).SpaceLeft -= size;
    }
    
    private void checkDirectoryCorrectness(string path)
    {
        // ^(([a-zA-Z0-9А-Яа-я_\s\-()\[\]{}.]+/)+)$
        // без / в начале и с / в конце
        Regex regex = new Regex(@"^(([a-zA-Z0-9А-Яа-я_\s\-()\[\]{}.]+/)+)$");
        if (!regex.IsMatch(path) || path.Length > 232)
            throw new AppException("Invalid directory path");
    }
    private bool tryCheckDirectoryCorrectness(string path)
    {
        Regex regex = new Regex(@"^(([a-zA-Z0-9А-Яа-я_\s\-()\[\]{}.]+/)+)$");
        if (!regex.IsMatch(path) || path.Length > 232)
            return false;
        return true;
    }
    private void checkFileNameCorrectness(string fileName)
    {
        Regex regex = new Regex(@"^[\w\-. \[\]\{\}\(\)]+$");
        if (!regex.IsMatch(fileName) || fileName.Length > 232)
            throw new AppException($"File name {fileName} is incorrect");
    }
    private bool tryCheckFileNameCorrectness(string fileName)
    {
        Regex regex = new Regex(@"^[\w\-. \[\]\{\}\(\)]+$");
        if (!regex.IsMatch(fileName) || fileName.Length > 232)
            return false;
        return true;
    }
}
