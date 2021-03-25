using Baitkm.BLL.Services.Geographic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class GeographicController : BaseController
    {
        private readonly IGeographicService _service;
        public GeographicController(IGeographicService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("{search}")]
        public async Task<IActionResult> City(string search)
        {
            return await MakeActionCallAsync(async () => await _service.City(search));
        }

        [HttpGet]
        [Route("{countryId}")]
        public IActionResult Cities(int countryId)
        {
            return MakeActionCall(() => _service.Cities(countryId));
        }

        [HttpGet]
        public IActionResult Countries()
        {
            return MakeActionCall(() => _service.Countries());
        }
    }
}