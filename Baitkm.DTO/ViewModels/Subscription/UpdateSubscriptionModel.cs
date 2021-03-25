using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Subscriptions;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Subscription
{
    public class UpdateSubscriptionModel : IViewModel
    {
        public IList<SubscriptionsType> Subscriptions { get; set; }
        public Language Language { get; set; }
        public AreaUnit AreaUnit { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
    }
}
