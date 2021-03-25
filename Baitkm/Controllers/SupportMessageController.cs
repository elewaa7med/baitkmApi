using Baitkm.Authorizations.Admins;
using Baitkm.BLL.Services.Conversations.SupportConversations.SupportMessages;
using Baitkm.DTO.ViewModels.Conversations.Messages;
using Baitkm.DTO.ViewModels.Conversations.SupportConversations.SupportMessages;
using Baitkm.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    public class SupportMessageController : BaseController
    {
        private readonly ISupportMessageService _service;
        public SupportMessageController(ISupportMessageService service)
        {
            _service = service;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Send([FromForm] SendSupportMessageModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Send(model,
                GetPersonId(), GetLanguage(), GetDeviceId()));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> GetAdminList([FromBody] MessagePagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetAdminList(model, GetPersonId()));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetMobileList([FromBody] MessagePagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetMobileList(model,
                GetPersonId(), GetLanguage(), GetDeviceId()));
        }
    }
}