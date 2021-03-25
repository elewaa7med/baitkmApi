using Baitkm.DTO.ViewModels.Conversations.Messages;
using Baitkm.DTO.ViewModels.Conversations.SupportConversations.SupportMessages;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Enums;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations.SupportConversations.SupportMessages
{
    public interface ISupportMessageService
    {
        Task<SupportMessageListModel> Send(SendSupportMessageModel model, int userId, Language language, string deviceId);
        Task<PagingResponseSupportMessageModel> GetAdminList(MessagePagingRequestModel model, int userId);
        Task<PagingResponseModel<SupportMessageListModel>> GetMobileList(MessagePagingRequestModel model, int userId,
             Language language, string deviceId);
    }
}
