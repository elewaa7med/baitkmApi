using Baitkm.Authorizations.UserGuests;
using Baitkm.BLL.Services.SaveFilters;
using Baitkm.DTO.ViewModels.SaveFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class SaveFilterController : BaseController
    {
        private readonly ISaveFilterService _saveFilterService;
        public SaveFilterController(ISaveFilterService saveFilterService)
        {
            _saveFilterService = saveFilterService;
        }

        [HttpPost]
        [UserGuestAuthorize]
        public async Task<IActionResult> Add([FromBody] AddSaveFilterViewModel model)
        {
            return await MakeActionCallAsync(async () => await _saveFilterService.Add(model, GetPersonId(),
                GetLanguage(), GetDeviceId()));
        }

        [HttpGet]
        [UserGuestAuthorize]
        public async Task<IActionResult> SaveFilterList()
        {
            return await MakeActionCallAsync(async () => await _saveFilterService.SaveFilterList(GetPersonId(),
                GetLanguage(), GetDeviceId()));
        }

        [HttpGet]
        [Route("{id}")]
        [UserGuestAuthorize]
        public async Task<IActionResult> SaveFilterById(int id)
        {
            return await MakeActionCallAsync(async () => await _saveFilterService.SaveFilterById(id, GetPersonId(),
                GetLanguage(), GetDeviceId()));
        }

        [HttpPut]
        [Route("{id}")]
        [UserGuestAuthorize]
        public async Task<IActionResult> Edit([FromBody] UpdateSaveFilterModel model, int id)
        {
            return await MakeActionCallAsync(async () => await _saveFilterService.Edit(model, GetPersonId(),
                id, GetLanguage(), GetDeviceId()));
        }

        [HttpDelete]
        [Route("{id}")]
        [UserGuestAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            return await MakeActionCallAsync(async () => await _saveFilterService.Delete(id, GetPersonId(),
                GetLanguage(), GetDeviceId()));
        }
    }
}