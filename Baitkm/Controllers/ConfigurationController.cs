using Baitkm.Authorizations.Admins;
using Baitkm.BLL.Services.Configurations;
using Baitkm.DTO.ViewModels.Configurations;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums;
using Baitkm.Enums.UserRelated;
using Baitkm.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    public class ConfigurationController : BaseController
    {
        private readonly IConfigurationService _service;
        public ConfigurationController(IConfigurationService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSettings()
        {
            return await MakeActionCallAsync(async () => await _service.GetSettings());
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetRules()
        {
            return await MakeActionCallAsync(async () => await _service.GetRules(GetLanguage()));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> Edit([FromBody] List<ConfigurationViewModel> model)
        {
            return await MakeActionCallAsync(async () => await _service.Edit(model));
        }

        [HttpDelete]
        [AdminAuthorize]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await MakeActionCallAsync(async () => await _service.Delete(id));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> UploadHomePageCoverImage([FromForm] UploadFileModel model)
        {
            return await MakeActionCallAsync(async () => await _service.UploadHomePageCoverImage(model));
        }

        [HttpGet]
        [AdminAuthorize]
        public async Task<IActionResult> GetHomePageCoverImageList()
        {
            return await MakeActionCallAsync(async () => await _service.GetHomePageCoverImageList());
        }

        [HttpDelete]
        [AdminAuthorize]
        [Route("{id}")]
        public async Task<IActionResult> RemovePhoto(int id)
        {
            return await MakeActionCallAsync(async () => await _service.RemovePhoto(GetLanguage(), id));
        }

        [HttpPut]
        [AdminAuthorize]
        [Route("{id}")]
        public async Task<IActionResult> BasePhoto(int id)
        {
            return await MakeActionCallAsync(async () => await _service.BasePhoto(GetLanguage(), id));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetBasePhoto()
        {
            return await MakeActionCallAsync(async () => await _service.GetBasePhoto());
        }
    }
}