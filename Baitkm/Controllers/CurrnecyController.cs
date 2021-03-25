using Baitkm.BLL.Services;
using Baitkm.BLL.Services.Currencies;
using Baitkm.DTO.ViewModels;
using Baitkm.Infrastructure.Helpers.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class CurrnecyController : BaseController
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly ICurrencyService currencyService;

        public CurrnecyController(IHttpClientFactory clientFactory,
            ICurrencyService currencyService)
		{
            this.clientFactory = clientFactory;
            this.currencyService = currencyService;
        }

        [HttpGet]
        public async  Task<ServiceResult> Get()
        {
            var client = clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://itfllc.am/api/rate/exchange?from=12&to=1&value=1");
            var response = await client.SendAsync(request);
            CurrencyResponseModel responseModel = new CurrencyResponseModel();


            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var res = result.Split('"', ',', ':');

                return new ServiceResult
                {
                    Data = new CurrencyResponseModel
                    {
                        Result = res[35],
                        Amount = res[39]
                    },
                    Success = true,
                };
            };

            return new ServiceResult();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return await MakeActionCallAsync(async () => await currencyService.List());
        }
    }
}