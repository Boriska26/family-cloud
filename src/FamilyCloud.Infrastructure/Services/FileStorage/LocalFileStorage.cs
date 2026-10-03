using FamilyCloud.Application.Services.FileStorage;
using FamilyCloud.Domain;
using Microsoft.Extensions.Options;

namespace FamilyCloud.Infrastructure.Services.FileStorage
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly FileStorageOptions _options;

        public LocalFileStorage(IOptions<FileStorageOptions> options)
        {
            _options = options.Value;
        }

        public async Task<string> SaveFileAsync(
            Stream stream,
            FileSaveOptions fileSaveOptions,
            CancellationToken cancellationToken)
        {
            string directoryName = "photos";
            string localPath = Path.Combine(directoryName, $"{fileSaveOptions.Id}");
            string path = Path.Combine(
                _options.RootPath,
                localPath);

            Directory.CreateDirectory(path);

            string variant = fileSaveOptions.Variant switch
            {
                PhotoVariant.Original => "original",
                PhotoVariant.Thumbnail => "thumbnail",
                _ => throw new InvalidOperationException()
            };
            string fileName = $"{variant}{fileSaveOptions.Extension}";

            string fullPath = Path.Combine(path, fileName);

            using FileStream fileStream = new FileStream(
                path: fullPath,
                mode: FileMode.CreateNew,
                access: FileAccess.Write);

            await stream.CopyToAsync(fileStream, cancellationToken);

            return $"{directoryName}/{fileSaveOptions.Id}/{fileName}";
        }

        public Stream OpenRead(string storageKey)
        {
            string filePath = Path.Combine(_options.RootPath, storageKey);
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return stream;
        }
    }
}