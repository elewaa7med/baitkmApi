using Baitkm.Authorizations.Admins;
using Baitkm.BLL.Services.FAQs;
using Baitkm.DTO.ViewModels.FAQ;
using Baitkm.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FAQController : BaseController
    {
        private readonly IFAQService _service;
        public FAQController(IFAQService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetList()
        {
            return await MakeActionCallAsync(async () => await _service.GetList());
        }

        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            return await MakeActionCallAsync(async () => await _service.Get(id));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> Add([FromBody] AddFAQModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Add(model));
        }

        [HttpPut]
        [AdminAuthorize]
        public async Task<IActionResult> Edit([FromBody] FAQViewModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Edit(model));
        }

        [HttpDelete]
        [Route("{id}")]
        [AdminAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            return await MakeActionCallAsync(async () => await _service.Delete(id));
        }
    }
}