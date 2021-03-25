using Baitkm.DAL.Context;
using Baitkm.DAL.Repository.Entities;
using Baitkm.Entities;
using Baitkm.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.Statistics
{
    public class ActivityLogger : IActivityLogger
    {
        public async Task Log(string userName)
        {
            using (var repo = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                var caller = await repo.Filter<User>(x => (x.PhoneCode + x.Phone) == userName || x.Email == userName && x.RoleEnum != Role.Admin).FirstOrDefaultAsync();
                if (caller != null)
                    repo.Create(new Statistic
                    {
                        ActivityDate = DateTime.UtcNow,
                        UserId = caller.Id
                    });
                await repo.SaveChangesAsync();
            }
        }
    }
}
