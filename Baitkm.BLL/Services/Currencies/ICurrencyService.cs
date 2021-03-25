using Baitkm.DTO.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Currencies
{
    public interface ICurrencyService
    {
        Task<List<CurrencyListResponseModel>> List();
    }
}