using Baitkm.DTO.ViewModels;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Baitkm.BLL.Services.Exchanges
{
    public class ExchangeService : IExchangeService
    {
        public decimal CurrencyRate(int exId, int requestId, decimal amount)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync($"https://itfllc.am/api/rate/exchange?from={requestId}&to={exId}&value={amount}").Result;
                var responseResult = JsonConvert.DeserializeObject<BaseExchangeResponseModel>(response.Content.ReadAsStringAsync().Result);
                if (responseResult.Message == "Bad Request")
                    return 1;

                return (Math.Round(responseResult.Data.Result, 3, MidpointRounding.ToEven));
            }
        }
    }
}