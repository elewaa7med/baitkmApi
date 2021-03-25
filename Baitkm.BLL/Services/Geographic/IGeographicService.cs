using Baitkm.DTO.ViewModels.Helpers.Matrix;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Geographic
{
    public interface IGeographicService
    {
        Task<List<LocateBaseResponseModel>> City(string city);
        IEnumerable<LocateBaseResponseModel> Cities(int countryId);
        IEnumerable<LocateBaseResponseModel> Countries();
    }
}
