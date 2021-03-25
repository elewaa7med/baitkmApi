using Baitkm.DTO.ViewModels.SaveFilters;
using Baitkm.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.SaveFilters
{
    public interface ISaveFilterService
    {
        Task<bool> Add(AddSaveFilterViewModel model, int userId, Language language, string deviceId);
        Task<bool> Edit(UpdateSaveFilterModel model, int userId, int id, Language language, string deviceId);
        Task<bool> Delete(int id, int userId, Language language, string deviceId);
        Task<SaveFilterViewModel> SaveFilterById(int id, int userId, Language language, string deviceId);
        Task<IEnumerable<SaveFilterViewModel>> SaveFilterList(int userId, Language language, string deviceId);
    }
}