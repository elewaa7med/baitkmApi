using Baitkm.BLL.Services.Conversations.Messages;
using Baitkm.DTO.ViewModels.Conversations.Messages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    public class MessageController : BaseController
    {
        private readonly IMessageService _service;
        public MessageController(IMessageService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] MessagePagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetList(model, GetPersonId()));
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromForm] SendMessageModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Send(model, GetPersonId(), GetLanguage(), GetDeviceId()));
        }
    }
}