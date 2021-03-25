namespace Baitkm.DTO.ViewModels.Bases
{
    public interface IIndividualNotificationBase : INotificationBase
    {
        int ReceiverId { get; set; }
    }
}