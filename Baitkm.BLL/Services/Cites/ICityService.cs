using Baitkm.DTO.ViewModels.Cities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Cites
{
    public interface ICityService
    {
        Task<bool> Add(CityAddModel model);
        Task<bool> Edit(CityEditModel model);
        Task<bool> Delete(int id);
        Task<CityViewModel> GetCityById(int id);
        Task<List<CityViewModel>> GetCityList();
    }
}