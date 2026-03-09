namespace ToplantiApp.Domain.Interfaces;

public interface IFileService
{
    Task<(string fileName, string filePath, long fileSize, bool isCompressed)> UploadFileAsync(
        Stream fileStream, string originalFileName, string contentType, string subFolder, bool compress = false);
    Task DeleteFileAsync(string filePath);
    Task<(byte[] data, string contentType, string fileName)> GetFileAsync(string filePath, bool isCompressed);
}
