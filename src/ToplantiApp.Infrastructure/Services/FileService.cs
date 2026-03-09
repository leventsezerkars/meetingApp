using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadsPath;
    private readonly ILogger<FileService> _logger;

    public FileService(IHostEnvironment environment, ILogger<FileService> logger)
    {
        _uploadsPath = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads");
        _logger = logger;

        if (!Directory.Exists(_uploadsPath))
            Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<(string fileName, string filePath, long fileSize, bool isCompressed)> UploadFileAsync(
        Stream fileStream, string originalFileName, string contentType, string subFolder, bool compress = false)
    {
        var folderPath = Path.Combine(_uploadsPath, subFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";

        if (compress)
            uniqueFileName += ".gz";

        var fullPath = Path.Combine(folderPath, uniqueFileName);
        var relativePath = Path.Combine("uploads", subFolder, uniqueFileName);

        if (compress)
        {
            await using var outputStream = File.Create(fullPath);
            await using var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal);
            await fileStream.CopyToAsync(gzipStream);
        }
        else
        {
            await using var outputStream = File.Create(fullPath);
            await fileStream.CopyToAsync(outputStream);
        }

        var fileInfo = new FileInfo(fullPath);
        _logger.LogInformation("Dosya yuklendi: {Path}, Boyut: {Size}, Sikistirildi: {Compressed}",
            relativePath, fileInfo.Length, compress);

        return (uniqueFileName, relativePath, fileInfo.Length, compress);
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadsPath, "..", filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Dosya silindi: {Path}", filePath);
        }

        return Task.CompletedTask;
    }

    public async Task<(byte[] data, string contentType, string fileName)> GetFileAsync(string filePath, bool isCompressed)
    {
        var fullPath = Path.Combine(_uploadsPath, "..", filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Dosya bulunamadi", filePath);

        byte[] data;
        var fileName = Path.GetFileName(filePath);

        if (isCompressed)
        {
            await using var fileStream = File.OpenRead(fullPath);
            await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            await gzipStream.CopyToAsync(memoryStream);
            data = memoryStream.ToArray();
            fileName = Path.GetFileNameWithoutExtension(fileName);
        }
        else
        {
            data = await File.ReadAllBytesAsync(fullPath);
        }

        var contentType = GetContentType(fileName);
        return (data, contentType, fileName);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
