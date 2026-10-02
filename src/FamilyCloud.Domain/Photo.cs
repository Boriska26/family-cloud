namespace FamilyCloud.Domain
{
    public class Photo
    {
        private Photo() { }

        public Photo(string sourceName, long size, string mimeType)
        {
            Id = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(sourceName))
                throw new ArgumentNullException(nameof(sourceName));
            if (size <= 0)
                throw new ArgumentNullException(nameof(size));
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentNullException(mimeType);

            SourceName = sourceName;
            Size = size;
            MimeType = mimeType;
        }

        public Guid Id { get; private set; }

        public string SourceName { get; private set; } = string.Empty;

        public string StorageKey { get; private set; } = string.Empty;

        public int Height { get; private set; }

        public int Width { get; private set; }

        public string MimeType { get; private set; } = string.Empty;

        public long Size { get; private set; }

        public DateTimeOffset UploadedAt { get; private set; }

        public void SetStorageKey(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentException(nameof(storageKey));
            }

            StorageKey = storageKey;
        }

        public void MarkAsUploadedAt(DateTimeOffset dateTime)
        {
            if (UploadedAt != default)
            {
                throw new InvalidOperationException("Date has uploaded date time");
            }

            UploadedAt = dateTime;
        }
    }
}