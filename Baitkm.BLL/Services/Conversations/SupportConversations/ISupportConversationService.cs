using Baitkm.DTO.ViewModels.Conversations.SupportConversations;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Enums;
using Baitkm.Enums.UserRelated;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations.SupportConversations
{
    public interface ISupportConversationService
    {
        Task<PagingResponseModel<SupportConversationListModel>> GetList(SupportConversationPagingRequestModel model, int userId);
        Task<int> Create(int userId, Language language, string deviceId);
    }
}
