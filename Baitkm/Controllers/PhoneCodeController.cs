using Baitkm.Authorizations.UserGuests;
using Baitkm.BLL.Services.PhoneCodes;
using Baitkm.DTO.ViewModels.PhoneCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class PhoneCodeController : BaseController
    {
        private readonly IPhoneCodeService _service;
        public PhoneCodeController(IPhoneCodeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            return await MakeActionCallAsync(async () => await _service.GetList());
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddPhoneCodeModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Add(model));
        }
    }
}