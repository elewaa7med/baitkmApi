using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.Managers
{
    public interface IRoleManager
    {
        Task<bool> CheckValidity(string userName, string verifiedBy);
    }
}