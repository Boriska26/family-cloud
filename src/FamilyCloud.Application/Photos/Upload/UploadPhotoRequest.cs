namespace FamilyCloud.Application.Photos.Upload
{
    public record UploadPhotoRequest(
        Stream Stream,
        string SourceName, 
        string MimeType,
        long Size);
}
