using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.Statistics
{
    public interface IActivityLogger
    {
        Task Log(string userName);
    }
}
