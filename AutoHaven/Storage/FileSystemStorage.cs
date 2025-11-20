using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace AutoHaven.Storage
{
    public class FileSystemStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;

        public FileSystemStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string SaveFile(IFormFile file, string relativeFolder)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            var webRoot = _env.WebRootPath ?? _env.ContentRootPath;
            var folder = Path.Combine(webRoot, relativeFolder.TrimStart('/', '\\'));
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(folder, fileName);

            using (var stream = new FileStream(full, FileMode.Create))
            {
                // synchronous copy
                file.CopyTo(stream);
            }

            var rel = Path.GetRelativePath(webRoot, full).Replace("\\", "/");
            return rel;
        }

        public bool DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;

            var webRoot = _env.WebRootPath ?? _env.ContentRootPath;
            var cleaned = relativePath.TrimStart('~', '/', '\\').Replace("/", Path.DirectorySeparatorChar.ToString());
            var full = Path.Combine(webRoot, cleaned);

            try
            {
                if (File.Exists(full)) File.Delete(full);
                return true;
            }
            catch
            {
                // log in real-life
                return false;
            }
        }
    }
}

