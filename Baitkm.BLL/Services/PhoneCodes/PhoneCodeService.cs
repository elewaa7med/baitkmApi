using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.PhoneCodes;
using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.PhoneCodes
{
    public class PhoneCodeService : IPhoneCodeService
    {
        private readonly IEntityRepository _repository;
        public PhoneCodeService(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PhoneCodeListModel>> GetList()
        {
            return await _repository.Filter<PhoneCode>(x => !x.IsDeleted).Select(x => new PhoneCodeListModel
            {
                Code = x.Code,
                Country = x.Country
            }).OrderBy(x => x.Country).ToListAsync();
        }

        public async Task<bool> Add(AddPhoneCodeModel model)
        {
            _repository.Create(new PhoneCode
            {
                Country = model.Country,
                Code = model.Code
            });
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
