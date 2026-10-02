using FamilyCloud.Application.Photos.Get;
using FamilyCloud.Application.Photos.Upload;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCloud.Api.Controllers
{
    [Route("api/photos")]
    public class PhotoController : Controller
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
        public async Task<IActionResult> GetPhotoById(Guid id, [FromServices] GetPhotoService getPhotoService, CancellationToken cancellationToken)
        {
            GetPhotoResult? getPhotoResult = await getPhotoService.GetByIdAsync(id, cancellationToken);
            if (getPhotoResult == null)
            {
                return NotFound("Фотография не найдена");
            }

            return File(getPhotoResult.PhotoStream, getPhotoResult.ContentType, getPhotoResult.FileName);
        }
    }
}