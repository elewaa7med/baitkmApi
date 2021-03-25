using Baitkm.DAL.Repository.Entities;
using Baitkm.Entities;
using Baitkm.Enums.UserRelated;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.Managers
{
    public class RoleManager : IRoleManager
    {
        private readonly IEntityRepository _repository;
        public RoleManager(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> CheckValidity(string userName, string verifiedBy)
        {
            Enum.TryParse(verifiedBy, out VerifiedBy verified);
            var caller = await _repository.Filter<User>(x => (x.Email == userName
                || (x.PhoneCode + x.Phone) == userName) && x.VerifiedBy == verified).FirstOrDefaultAsync();
            if (caller == null)
                return false;
            if (caller.IsBlocked)
                return false;
            return true;
        }
    }
}