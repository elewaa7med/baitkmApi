using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Cities;
using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Cites
{
    public class CityService : ICityService
    {
        private readonly IEntityRepository _repository;
        public CityService(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Add(CityAddModel model)
        {
            _repository.Create(new City
            {
                Name = model.Name
            });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Edit(CityEditModel model)
        {
            var country = await _repository.Filter<City>(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (country == null)
                throw new Exception("Country not found");
            country.Name = model.Name;
            _repository.Update(country);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<CityViewModel> GetCityById(int id)
        {
            return await _repository.Filter<City>(x => x.Id == id).Select(x => new CityViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).FirstOrDefaultAsync();
        }

        public async Task<List<CityViewModel>> GetCityList()
        {
            return await _repository.GetAll<City>().Select(x => new CityViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<bool> Delete(int id)
        {
            var country = await _repository.Filter<City>(x => !x.IsDeleted && x.Id == id).FirstOrDefaultAsync();
            if (country == null)
                throw new Exception("Country not found");
            country.IsDeleted = true;
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
