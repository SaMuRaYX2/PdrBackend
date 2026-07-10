using Microsoft.Extensions.Configuration;
using TrafficRules.Application.Interfaces;

namespace TrafficRules.Application.Services;

public class LocalImageStorageService : IImageStorageService
{
    private readonly IConfiguration _configuration;

    public LocalImageStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string extension, CancellationToken cancellationToken = default)
    {
        var relativePath = _configuration["QuestionImagesPath"] ?? "wwwroot/images";
        var directory = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        Directory.CreateDirectory(directory);

        var fileName = Guid.NewGuid() + extension;
        var filePath = Path.Combine(directory, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await imageStream.CopyToAsync(fileStream, cancellationToken);

        var urlPath = relativePath.Replace("wwwroot", "").Replace("\\", "/") + "/" + fileName;
        if (!urlPath.StartsWith("/")) urlPath = "/" + urlPath;

        return urlPath;
    }
}
