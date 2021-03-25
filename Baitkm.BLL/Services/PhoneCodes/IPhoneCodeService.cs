using Baitkm.DTO.ViewModels.PhoneCodes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.PhoneCodes
{
    public interface IPhoneCodeService
    {
        Task<List<PhoneCodeListModel>> GetList();
        Task<bool> Add(AddPhoneCodeModel model);
    }
}