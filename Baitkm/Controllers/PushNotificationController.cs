using Baitkm.BLL.Services.Notifications;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.PushNotifications;
using Baitkm.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class PushNotificationController : BaseController
    {
        private readonly IPushNotificationService _service;
        public PushNotificationController(IPushNotificationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetList(model));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePushNotificationModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Create(model));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await MakeActionCallAsync(async () => await _service.Delete(id));
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            return await MakeActionCallAsync(async () => await _service.Details(id));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePushNotificationModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Update(model));
        }
    }
}