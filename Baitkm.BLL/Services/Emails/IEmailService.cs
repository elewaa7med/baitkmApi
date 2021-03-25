using Baitkm.DTO.ViewModels.Email;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Emails
{
    public interface IEmailService
    {
        Task<bool> Add(AddEmailRequestModel model);
        Task<bool> Delete(int id);
        Task<bool> Edit(EditEmailRequestModel model);
        Task<EmailResponseModel> Get(int id);
        Task<List<EmailResponseModel>> List();
    }
}