namespace FamilyCloud.Domain
{
    public class Photo
    {
        private readonly List<PhotoFile> _files = [];

        private Photo() { }

        private Photo(string sourceName)
        {
            Id = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(sourceName))
                throw new ArgumentNullException(nameof(sourceName));

            SourceName = sourceName;
        }

        public Guid Id { get; private set; }

        public string SourceName { get; private set; } = string.Empty;

        public DateTimeOffset UploadedAt { get; private set; }

        public IReadOnlyCollection<PhotoFile> Files => _files;

        public void MarkAsUploadedAt(DateTimeOffset dateTime)
        {
            if (UploadedAt != default)
            {
                throw new InvalidOperationException("Date has uploaded date time");
            }

            UploadedAt = dateTime;
        }

        public static Photo Create(string sourceName, long size, string mimeType)
        {
            Photo photo = new Photo(sourceName);
            photo.AddFile(PhotoVariant.Original, size, mimeType);

            return photo;
        }

        public PhotoFile AddFile(PhotoVariant variant, long size, string mimeType)
        {
            if (_files.Any(n => n.Variant == variant))
            {
                throw new InvalidOperationException($"Файл варианта {variant} уже существует");
            }

            PhotoFile photoFile = new PhotoFile(variant, size, mimeType, Id);
            _files.Add(photoFile);

            return photoFile;
        }

        public PhotoFile GetOriginalFile()
        {
            PhotoFile? originalFile = GetFile(PhotoVariant.Original);
            if (originalFile == null)
            {
                throw new InvalidOperationException($"Файл {PhotoVariant.Original} не существует");
            }

            return originalFile;
        }

        public PhotoFile? GetFile(PhotoVariant variant)
        {
            return _files.FirstOrDefault(n => n.Variant == variant);
        }
    }
}