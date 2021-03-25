using Baitkm.DTO.ViewModels.Conversations;
using Baitkm.DTO.ViewModels.Conversations.Messages;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Enums.Notifications;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations
{
    public interface IConversationService
    {
        Task<int> Add(int announcementId, int userId);
        Task<PagingResponseModel<ConversationListModel>> GetList(ConversationPagingRequestModel model, int userId, OsType osType);
        Task<bool> Delete(int conversationId, int userId);
    }
}