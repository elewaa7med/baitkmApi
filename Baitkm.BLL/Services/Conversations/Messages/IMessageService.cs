using Baitkm.DTO.ViewModels.Conversations.Messages;
using Baitkm.Enums;
using Baitkm.Enums.UserRelated;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations.Messages
{
    public interface IMessageService
    {
        Task<PagingResponseMessageModel> GetList(MessagePagingRequestModel model, int userId);
        Task<MessageListModel> Send(SendMessageModel model, int userId, Language languagem, string deviceId);
        //Task<bool> Delete(MessageDeleteModel model, string userName);
    }
}
