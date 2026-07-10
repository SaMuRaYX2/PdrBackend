namespace TrafficRules.Application.Interfaces;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(Stream imageStream, string extension, CancellationToken cancellationToken = default);
}
