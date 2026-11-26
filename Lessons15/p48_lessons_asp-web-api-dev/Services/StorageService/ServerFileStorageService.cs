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

    public async Task<bool> DeleteFileAsync(string url)
    {
        throw new NotImplementedException();
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