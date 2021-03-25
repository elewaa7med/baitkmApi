using Baitkm.BLL.Services.Conversations.SupportConversations;
using Baitkm.DTO.ViewModels.Conversations.SupportConversations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class SupportConversationController : BaseController
    {
        private readonly ISupportConversationService _service;
        public SupportConversationController(ISupportConversationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] SupportConversationPagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetList(model, GetPersonId()));
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            return await MakeActionCallAsync(async () => await _service.Create(GetPersonId(), GetLanguage(), GetDeviceId()));
        }
    }
}