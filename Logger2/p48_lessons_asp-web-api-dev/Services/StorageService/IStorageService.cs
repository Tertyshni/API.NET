namespace p48_lessons_asp.Services.StorageService;

public interface IStorageService
{
    Task<string> SaveFileAsync(string entityType, string entityName, IFormFile file, string fileName=null);
    Task<bool> DeleteFileAsync(string url);
}