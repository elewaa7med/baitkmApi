using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Conversations;
using System;
using System.Threading.Tasks;

namespace Baitkm.DTO.ViewModels.Conversations.Messages
{
    public class MessageListModel : IViewModel
    {
        public MessageBodyType MessageBodyType { get; set; }
        public int SenderId { get; set; }
        public ImageOptimizer Photo { get; set; }
        public int MessageId { get; set; }
        public string MessageText { get; set; }
        public string FileUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsSentFromMe { get; set; }
        public long FileSize { get; set; }
        public string FullName { get; set; }
        public ImageOptimizer ProfilePhoto { get; set; }
        public MessageListModel ReplayMessage { get; set; }
    }
}