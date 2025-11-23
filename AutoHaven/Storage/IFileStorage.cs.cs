using Microsoft.AspNetCore.Http;

public interface IFileStorage
{
    /// <summary>Save file synchronously and return a relative path (e.g., "uploads/listings/xxx.jpg").</summary>
    string SaveFile(IFormFile file, string relativeFolder);

    /// <summary>Delete file synchronously. Return true if deleted or didn't exist.</summary>
    bool DeleteFile(string relativePath);
}
