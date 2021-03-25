using Baitkm.Authorizations.UserGuests;
using Baitkm.BLL.Services.Subscribes;
using Baitkm.DTO.ViewModels.Subscribes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class SubscribeController : BaseController
    {
        private readonly ISubscribeService subscribeService;

        public SubscribeController(ISubscribeService subscribeService)
        {
            this.subscribeService = subscribeService;
        }

        [HttpPost]
        [UserGuestAuthorize]
        public async Task<IActionResult> Subscribe([FromBody] AddSubscribeRequestModel model)
        {
            return await MakeActionCallAsync(async () =>
                await subscribeService.SubscribeAsync(model, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        [HttpPost]
        [UserGuestAuthorize]
        public async Task<IActionResult> UnSubscribe([FromQuery] int announcementId)
        {
            return await MakeActionCallAsync(async () =>
                await subscribeService.UnSubscribeAsync(announcementId, GetPersonId(), GetDeviceId(), GetLanguage()));
        }
    }
}