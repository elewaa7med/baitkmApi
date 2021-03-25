using Baitkm.BLL.Services;
using Baitkm.BLL.Services.Emails;
using Baitkm.DTO.ViewModels.Email;
using Baitkm.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class EmailController : BaseController
    {
        private readonly IEmailService emailService;

        public EmailController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddEmailRequestModel model)
        {
            return await MakeActionCallAsync(async () => await emailService.Add(model));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            return await MakeActionCallAsync(async () => await emailService.Delete(id));
        }

        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] EditEmailRequestModel model)
        {
            return await MakeActionCallAsync(async () => await emailService.Edit(model));
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int id)
        {
            return await MakeActionCallAsync(async () => await emailService.Get(id));
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return await MakeActionCallAsync(async () => await emailService.List());
        }
    }
}