namespace FamilyCloud.Application.Services.FileStorage
{
    public interface IImageProcessor
    {
        Stream CreateWebpThumbnail(Stream source,
            int maxWidth,
            int maxHeight,
            int quality);
    }
}
