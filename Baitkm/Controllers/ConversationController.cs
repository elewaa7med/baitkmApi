using Baitkm.BLL.Services.Conversations;
using Baitkm.DTO.ViewModels.Conversations.Messages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    public class ConversationController : BaseController
    {
        private readonly IConversationService _service;
        public ConversationController(IConversationService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("{announcementId}")]
        public async Task<IActionResult> Add(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.Add(announcementId, GetPersonId()));
        }

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] ConversationPagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetList(model, GetPersonId(), GetOsType()));
        }

        [HttpDelete]
        [Route("{conversationId}")]
        public async Task<IActionResult> Delete(int conversationId)
        {
            return await MakeActionCallAsync(async () => await _service.Delete(conversationId, GetPersonId()));
        }
    }
}