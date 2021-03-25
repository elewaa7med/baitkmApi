using Baitkm.Enums.Attachments;
using Baitkm.MediaServer.BLL.Services.Files;
using Baitkm.MediaServer.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.MediaServer.API.Controllers
{
    [AllowAnonymous]
    public class ImageController : BaseController
    {
        private readonly IFilesService _service;
        public ImageController(IFilesService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("{type}/{fileName}/{isBlur}/{id}")]
        [ResponseCache(Duration = 10000)]
        public async Task<IActionResult> Download(UploadType type, string fileName, bool isBlur, int id)
        {
            var result = await _service.Download(type, fileName, id, isBlur);
            if (result == null)
                return NotFound();
            var stream = new MemoryStream(result.Bytes);
            if (result.ContentType.StartsWith("video"))
                HttpContext.Response.StatusCode = StatusCodes.Status206PartialContent;
            return File(stream, result.ContentType, true);
        }

        [HttpPost]
        [ResponseCache(Duration = 10000)]
        public IActionResult DownloadFromSocialPage([FromBody] GetPhotoFromSocialModel model)
        {
            var name = _service.DownloadFromSocialPage(model.Uri);
            return Json(name);
        }

        [HttpGet]
        [Route("{type}/{fileName}/{width}/{height}/{isBlur}/{id}")]
        [ResponseCache(Duration = 10000)]
        public async Task<IActionResult> Resize(UploadType type, string fileName, int width, int height, bool isBlur, int id)
        {
            var result = await _service.Resize(type, fileName, width, height, id, isBlur);
            if (result == null)
                return NotFound();
            var stream = new MemoryStream(result.Bytes);
            return File(stream, result.ContentType, true);
        }

        [HttpPost]
        [Route("{type}/{isRelativeRequested}/{id}")]
        public async Task<IActionResult> Upload(UploadType type, bool isRelativeRequested, int id, [FromForm] UploadFileModel model)
        {
            return await MakeActionCallWithModelAsync(async () => await _service.FileUploader(model, type, isRelativeRequested, id), model);
        }

        [HttpPost]
        [Route("{type}/{isRelativeRequested}")]
        public IActionResult UploadMultiple(UploadType type, bool isRelativeRequested, [FromForm] MultipleFileUploadModel model)
        {
            HttpContext.Request.Headers.TryGetValue("announcementId", out var announcementIdValue);
            HttpContext.Request.Headers.TryGetValue("announcementPhotoType", out var announcementPhotoTypeValue);
            int.TryParse(announcementIdValue.FirstOrDefault(), out var announcementId);
            Enum.TryParse(announcementPhotoTypeValue.FirstOrDefault(), out AttachmentType announcementPhotoType);
            return MakeActionCallWithModel(() => _service.MultipleFileUploader(model, type, announcementPhotoType, isRelativeRequested, announcementId), model);
        }

        [HttpDelete]
        [Route("{type}/{fileName}/{id}")]
        public IActionResult Remove(UploadType type, string fileName, int id)
        {
            return MakeActionCall(() => _service.RemoveFile(fileName, type, id));
        }

        [HttpGet]
        [Route("{type}/{fileName}/{isBlur}/{id}")]
        public async Task<IActionResult> DownloadFile(UploadType type, string fileName, bool isBlur, int id)
        {
            var result = await _service.Download(type, fileName, id, isBlur);
            if (result == null)
                return NotFound();
            var stream = new MemoryStream(result.Bytes);
            return File(stream, result.ContentType, fileName);
        }

        [HttpGet]
        [Route("{type}/{fileName}/{id}")]
        public IActionResult GetLength(UploadType type, string fileName, int id)
        {
            return MakeActionCall(() => _service.GetLength(fileName, type, id));
        }

        [HttpPost]
        public IActionResult DownloadMap([FromBody] DownloadMapModel model)
        {
            return MakeActionCallWithModel(() => _service.DownloadMap(model), model);
        }
    }
}