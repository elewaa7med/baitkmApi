using Baitkm.DTO.ViewModels.FAQ;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.FAQs
{
    public interface IFAQService
    {
        Task<bool> Add(AddFAQModel model);
        Task<bool> Edit(FAQViewModel model);
        Task<FAQViewModel> Get(int id);
        Task<List<FAQViewModel>> GetList();
        Task<bool> Delete(int id);
    }
}