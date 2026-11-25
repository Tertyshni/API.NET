using p48_lessons_asp.Services.Slug;

namespace p48_lessons_asp.Services.StorageService;

public class ServerFileStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ServerFileStorageService> _logger;
    private readonly ISlugService _slugService;
    public ServerFileStorageService(IWebHostEnvironment env, ILogger<ServerFileStorageService> logger, ISlugService slugService)
    {
        _env = env;
        _logger = logger;
        _slugService = slugService;
    }

    public Task<bool> DeleteFileAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(_env.WebRootPath))
            {
                return Task.FromResult(false);
            }

            var rootPath = Path.GetFullPath(_env.WebRootPath);
            var relativePath = Uri.UnescapeDataString(url.TrimStart('/', '\\'))
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            var filePath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
            var pathFromRoot = Path.GetRelativePath(rootPath, filePath);

            // Не дозволяємо видаляти файли за межами wwwroot.
            if (pathFromRoot == ".." || pathFromRoot.StartsWith(".." + Path.DirectorySeparatorChar)
                || Path.IsPathRooted(pathFromRoot))
            {
                _logger.LogWarning("Attempt to delete a file outside wwwroot: {Url}", url);
                return Task.FromResult(false);
            }

            // Якщо файл вже відсутній, запис у БД все одно можна видалити.
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting file {Url}", url);
            return Task.FromResult(false);
        }
    }

    public async Task<string> SaveFileAsync(string entityType, string entityName, IFormFile file, string fileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is not send or empty");
            }
            var slugEntityType = _slugService.GetSlug(entityType); // Product, Category, User
            var slugEntityName = _slugService.GetSlug(entityName); // productName, categoryname

            var uploadFolderPath = Path.Combine(_env.WebRootPath, "images", slugEntityType, slugEntityName);

            if (!Directory.Exists(uploadFolderPath)) Directory.CreateDirectory(uploadFolderPath);

            var extension = Path.GetExtension(file.FileName);
            string resultFileName;

            if (!string.IsNullOrEmpty(fileName)) resultFileName = $"{fileName}{extension}";
            else resultFileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(uploadFolderPath, resultFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/images/{slugEntityType}/{slugEntityName}/{resultFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while try to save file");
            throw new Exception($"Error while try to save file: {ex.Message}", ex);
        }
    }
}