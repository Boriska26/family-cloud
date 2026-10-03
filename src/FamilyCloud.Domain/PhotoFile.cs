namespace FamilyCloud.Domain
{
    public class PhotoFile
    {
        private PhotoFile() { }

        internal PhotoFile(
            PhotoVariant variant,
            long size,
            string mimeType,
            Guid photoId)
        {
            Id = Guid.NewGuid();
            if (size <= 0)
                throw new ArgumentException(nameof(size));
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException(nameof(mimeType));
            Size = size;
            MimeType = mimeType;
            Variant = variant;
            PhotoId = photoId;
        }

        public Guid PhotoId { get; private set; }

        public Guid Id { get; private set; }

        public PhotoVariant Variant { get; private set; }

        public long Size { get; private set; }

        public string MimeType { get; private set; } = string.Empty;

        public string StorageKey { get; private set; } = string.Empty;

        public void SetStorageKey(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentException(nameof(storageKey));
            }

            StorageKey = storageKey;
        }
    }
}
