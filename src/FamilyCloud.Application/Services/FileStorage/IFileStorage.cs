namespace FamilyCloud.Application.Services.FileStorage
{
    public interface IFileStorage
    {
        Task<string> SaveFileAsync(
            Stream stream,
            FileSaveOptions options,
            CancellationToken cancellationToken);
    }
}