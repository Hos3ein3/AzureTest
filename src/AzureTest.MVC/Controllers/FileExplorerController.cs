using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class FileExplorerController : Controller
{
    private readonly IWebHostEnvironment _env;

    public FileExplorerController(IWebHostEnvironment env)
    {
        _env = env;
    }

    private string GetSafePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) relativePath = "";
        relativePath = relativePath.Replace("\\", "/").TrimStart('/');
        var basePath = _env.WebRootPath;
        var fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid path.");
        }

        return fullPath;
    }

    private string GetRelativePath(string fullPath)
    {
        var basePath = _env.WebRootPath;
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            return "";
        return fullPath.Substring(basePath.Length).Replace("\\", "/").TrimStart('/');
    }

    public IActionResult Index(string path = "")
    {
        try
        {
            var fullPath = GetSafePath(path);
            if (!Directory.Exists(fullPath))
            {
                return NotFound("Directory not found.");
            }

            var dirInfo = new DirectoryInfo(fullPath);
            var folders = dirInfo.GetDirectories().Select(d => new FileSystemItem(
                d.Name,
                GetRelativePath(d.FullName),
                true,
                GetDirectorySize(d),
                d.LastWriteTime,
                d.Extension
            )).OrderBy(f => f.Name).ToList();

            var files = dirInfo.GetFiles().Select(f => new FileSystemItem(
                f.Name,
                GetRelativePath(f.FullName),
                false,
                f.Length,
                f.LastWriteTime,
                f.Extension
            )).OrderBy(f => f.Name).ToList();

            folders.AddRange(files);

            ViewBag.CurrentPath = GetRelativePath(fullPath);
            ViewBag.ParentPath = GetRelativePath(dirInfo.Parent?.FullName ?? _env.WebRootPath);
            ViewBag.TotalVolume = GetDirectorySize(dirInfo);
            if (fullPath.Equals(_env.WebRootPath, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ParentPath = null;
            }

            return View(folders);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult CreateFolder(string path, string folderName)
    {
        try
        {
            var parentPath = GetSafePath(path);
            var newPath = Path.Combine(parentPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult Rename(string path, string newName)
    {
        try
        {
            var fullPath = GetSafePath(path);
            var parent = Path.GetDirectoryName(fullPath);
            var destPath = Path.Combine(parent, newName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Move(fullPath, destPath);
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Move(fullPath, destPath);
            }
            else
            {
                return NotFound();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult Delete(string path)
    {
        try
        {
            var fullPath = GetSafePath(path);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult Paste(string sourcePath, string targetPath, string action) // action = "copy" or "cut"
    {
        try
        {
            var source = GetSafePath(sourcePath);
            var targetDir = GetSafePath(targetPath);
            var itemName = Path.GetFileName(source);
            var dest = Path.Combine(targetDir, itemName);

            if (System.IO.File.Exists(source))
            {
                if (action == "cut") System.IO.File.Move(source, dest);
                else System.IO.File.Copy(source, dest, true);
            }
            else if (Directory.Exists(source))
            {
                if (action == "cut") Directory.Move(source, dest);
                else CopyDirectory(source, dest);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Upload(string path, List<IFormFile> files)
    {
        try
        {
            var targetDir = GetSafePath(path);
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(targetDir, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (var subDir in dir.GetDirectories())
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    private long GetDirectorySize(DirectoryInfo d)
    {
        try
        {
            long size = 0;
            foreach (var fi in d.GetFiles()) size += fi.Length;
            foreach (var di in d.GetDirectories()) size += GetDirectorySize(di);
            return size;
        }
        catch { return 0; }
    }
}

record FileSystemItem(string Name, string Path, bool IsDirectory, long Size, DateTime Modified, string Extension);