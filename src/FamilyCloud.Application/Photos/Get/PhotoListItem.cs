namespace FamilyCloud.Application.Photos.Get
{
    public record PhotoListItem(
        Guid Id,
        string FileName,
        DateTimeOffset UploadedAt);
}
