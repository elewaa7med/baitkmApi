using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Conversations;

namespace Baitkm.DTO.ViewModels.Conversations.SupportConversations
{
    public class SupportConversationListModel : IViewModel
    {
        public int Id { get; set; }
        public string ParticipantId { get; set; }
        public string FullName { get; set; }
        public ImageOptimizer Photo { get; set; }
        public int UnSeenCount { get; set; }
        public string MessageText { get; set; }
        public SupportMessageBodyType SupportMessageBodyType { get; set; }
    }
}
