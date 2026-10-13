using FamilyCloud.Application.Exceptions;
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
                throw new InvalidPhotoException("Не удалось декодировать изображение");
            }

            int newWidth = originalBitmap.Width;
            int newHeight = originalBitmap.Height;
            if (newWidth > maxWidth || newHeight > maxHeight)
            {
                double ratio = Math.Min((double)maxWidth / (double)newWidth, (double)maxHeight / (newHeight));
                newWidth = Math.Max(1, (int)(originalBitmap.Width * ratio));
                newHeight = Math.Max(1, (int)(originalBitmap.Height * ratio));
            }

            var imageInfo = new SKImageInfo(newWidth, newHeight);
            using var resizeBitmap = originalBitmap.Resize(imageInfo, SKSamplingOptions.Default);

            if (resizeBitmap == null)
            {
                throw new InvalidOperationException("Ошибка изменения размера файла");
            }

            using var finalImage = SKImage.FromBitmap(resizeBitmap);

            var skData = finalImage.Encode(SKEncodedImageFormat.Webp, quality);

            if(skData == null)
            {
                throw new InvalidOperationException("Неудалось закодировать изображение");
            }

            return skData.AsStream(true);
        }
    }
}