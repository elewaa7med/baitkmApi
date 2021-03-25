using Baitkm.DTO.ViewModels.Configurations;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Entities;
using Baitkm.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Configurations
{
    public interface IConfigurationService
    {
        Task<bool> Edit(List<ConfigurationViewModel> model);
        Task<bool> Delete(int id);
        Task<List<ConfigurationViewModel>> GetSettings();
        Task<RulesViewModel> GetRules(Language language);
        Task<ImageOptimizer> UploadHomePageCoverImage(UploadFileModel model);
        Task<List<GetHomePageListModel>> GetHomePageCoverImageList();
        Task<bool> RemovePhoto(Language language, int id);
        Task<bool> BasePhoto(Language language, int id);
        Task<ImageOptimizer> GetBasePhoto();
        NgrokSettings GetNgrokSettings();
        Currency Currencies();
        CurrnecySymbols CurrnecySymbols();
    }
}