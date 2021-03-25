using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.IpAddress
{
    public interface ILocateByIpAddress
    {
        //Task<LocateModel> Locate(string userName, string ip, string verifiedBy);
        Task Locate(string userName, string ip, string verifiedBy);
    }
}