using FamilyCloud.Application.Services.FileStorage;
using SkiaSharp;

namespace FamilyCloud.Infrastructure.Services.ImageProcessing
{
    public class SkiaImageProcessor : IImageProcessor
    {
        public Stream CreateWebpThumbnail(Stream source, int maxWidth, int maxHeight, int quality)
        {
            using var originalBitmap = SKBitmap.Decode(source);

            if (originalBitmap == null)
            {
                throw new InvalidOperationException("Не удалось декодировать изображение");
            }

            int newWidth = originalBitmap.Width;
            int newHeight = originalBitmap.Height;
            if (newWidth > maxWidth || newHeight > maxHeight)
            {
                double ratio = Math.Min((double)maxWidth / (double)newWidth, (double)maxHeight / (newHeight));
                newWidth = (int)(originalBitmap.Width * ratio);
                newHeight = (int)(originalBitmap.Height * ratio);
            }

            var imageInfo = new SKImageInfo(newWidth, newHeight);
            using var resizeBitmap = originalBitmap.Resize(imageInfo, SKSamplingOptions.Default);

            if (resizeBitmap == null)
            {
                throw new InvalidOperationException("Ошибка изменения размера файла");
            }

            using var finalImage = SKImage.FromBitmap(resizeBitmap);

            var skData = finalImage.Encode(SKEncodedImageFormat.Webp, quality);

            return skData.AsStream(true);
        }
    }
}