using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels;
using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IEntityRepository repository;

        public CurrencyService(IEntityRepository repository)
        {
            this.repository = repository;
        }

        public async Task<List<CurrencyListResponseModel>> List()
        {
            return await repository
                .FilterAsNoTracking<Currency>(c => !c.IsDeleted)
                .Select(c => new CurrencyListResponseModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Symbol = c.Symbol
                }).ToListAsync();
        }
    }
}