using Baitkm.BLL.Services.Cites;
using Baitkm.DTO.ViewModels.Cities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class CityController : BaseController
    {
        private readonly ICityService _cityService;
        public CityController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody]CityAddModel model)
        {
            return await MakeActionCallAsync(async () => await _cityService.Add(model));
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            return await MakeActionCallAsync(async () => await _cityService.GetCityList());
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            return await MakeActionCallAsync(async () => await _cityService.GetCityById(id));
        }

        [HttpPut]
        public async Task<IActionResult> Edit([FromBody]CityEditModel model)
        {
            return await MakeActionCallAsync(async () => await _cityService.Edit(model));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await MakeActionCallAsync(async () => await _cityService.Delete(id));
        }
    }
}