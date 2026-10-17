using FamilyCloud.Application.Common;
using FamilyCloud.Application.Photos.CreateThumbnail;
using FamilyCloud.Application.Photos.Get;
using FamilyCloud.Application.Photos.Upload;
using FamilyCloud.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCloud.Api.Controllers
{
    [ApiController]
    [Route("api/photos")]
    public class PhotoController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> UploadPhoto(
            IFormFile photo,
            [FromServices] UploadPhotoService uploadPhotoService,
            CancellationToken cancellationToken)
        {
            long size = photo.Length;
            string mimeType = photo.ContentType;
            string fileName = photo.FileName;
            using var stream = photo.OpenReadStream();
            UploadPhotoRequest uploadPhotoRequest = new(stream, fileName, mimeType, size);
            Guid newId = await uploadPhotoService.UploadAsync(uploadPhotoRequest, cancellationToken);

            return Ok(newId);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPhotoById(
            Guid id,
            [FromServices] GetPhotoService getPhotoService,
            CancellationToken cancellationToken)
        {
            GetPhotoResult? getPhotoResult = await getPhotoService.GetFileAsync(id, PhotoVariant.Original, cancellationToken);
            if (getPhotoResult == null)
            {
                return NotFound("Фотография не найдена");
            }

            return File(getPhotoResult.PhotoStream, getPhotoResult.ContentType, getPhotoResult.FileName);
        }

        [HttpGet]
        public async Task<ActionResult<PageResult<PhotoListItem>>> GetPageAsync(
            [FromServices] GetPhotoService getPhotoService,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await getPhotoService.GetPageAsync(page, pageSize, cancellationToken);

            return Ok(result);
        }

        [HttpPost("{id:guid}/thumbnail")]
        public async Task<IActionResult> CreateThumbnail(
            Guid id,
            [FromServices] CreateThumbnailService createThumbnailService,
            CancellationToken cancellationToken)
        {
            await createThumbnailService.CreateAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpGet("{id:guid}/thumbnail")]
        public async Task<IActionResult> GetThumbnail(
            Guid id,
            [FromServices] GetPhotoService getPhotoService,
            CancellationToken cancellationToken)
        {
            GetPhotoResult? getPhotoResult = await getPhotoService.GetFileAsync(id, PhotoVariant.Thumbnail, cancellationToken);

            if (getPhotoResult == null)
            {
                return NotFound();
            }

            return File(getPhotoResult.PhotoStream, getPhotoResult.ContentType, getPhotoResult.FileName);
        }
    }
}