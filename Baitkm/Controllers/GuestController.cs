using Baitkm.Authorizations.Guests;
using Baitkm.BLL.Services.Users.Guests;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Subscription;
using Baitkm.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class GuestController : BaseController
    {
        private readonly IGuestService _service;
        public GuestController(IGuestService service)
        {
            _service = service;
        }

        [HttpGet]
        [GuestAuthorize]
        public async Task<IActionResult> GuestProfile()
        {
            return await MakeActionCallAsync(async () => await _service.GuestProfileAsync(GetLanguage(), GetDeviceId()));
        }

        [HttpPost]
        public async Task<IActionResult> AddGuest()
        {
            return await MakeActionCallAsync(async () => await _service.AddGuestAsync(GetDeviceId(), GetDeviceToken(),
                GetOsType(), GetLanguage(), UserCurrency));
        }

        [HttpGet]
        [GuestAuthorize]
        public async Task<IActionResult> GetSupportConversationId()
        {
            return await MakeActionCallAsync(async () => await _service.GetSupportConversationId(GetLanguage(), GetDeviceId()));
        }

        [HttpPut]
        [GuestAuthorize]
        public async Task<IActionResult> EditSubscription([FromBody] UpdateSubscriptionModel model)
        {
            return await MakeActionCallAsync(async () => await _service.EditSubscription(model, GetDeviceId()));
        }

        [HttpGet]
        [GuestAuthorize]
        public async Task<IActionResult> GetSubscriptionList()
        {
            return await MakeActionCallAsync(async () => await _service.GetSubscription(GetDeviceId()));
        }

        [HttpPost]
        [GuestAuthorize]
        public async Task<IActionResult> GuestNotificationList([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await
                _service.GuestNotificationList(model, GetDeviceId(), GetLanguage()));
        }
    }
}