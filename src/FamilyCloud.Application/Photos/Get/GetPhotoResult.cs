namespace FamilyCloud.Application.Photos.Get
{
    public record GetPhotoResult(Stream PhotoStream, string ContentType, string FileName);
}
