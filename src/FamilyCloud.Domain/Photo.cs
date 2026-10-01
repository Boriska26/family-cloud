namespace FamilyCloud.Domain
{
    public class Photo
    {
        public Photo() { }

        public Guid Id { get; set; }

        public string SourceName { get; set; } = string.Empty;

        public string StorageKey { get; set; } = string.Empty;

        public int Height { get; set; }

        public int Width { get; set; }

        public string MimeType { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTimeOffset UploadedAt { get; set; }
    }
}