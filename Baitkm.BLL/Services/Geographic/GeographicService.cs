using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Helpers.Matrix;
using Baitkm.DTO.ViewModels.Helpers.Matrix.ParserHelpers;
using Baitkm.Entities;
using Baitkm.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Geographic
{
    public class GeographicService : IGeographicService
    {
        private readonly IEntityRepository _repository;
        public GeographicService(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LocateBaseResponseModel>> City(string city)
        {
            var jsonString = await city.MakeCall();
            var token = JToken.Parse(jsonString);
            var jsonList = token.SelectToken("predictions").ToString();
            var parsed = JsonConvert.DeserializeObject<List<ParsingBaseModel>>(jsonList);
            var result = new List<LocateBaseResponseModel>();
            foreach (var variable in parsed)
            {
                var cityName = variable.AddressModel.Address;
                var dbCity = await _repository.Filter<City>(x => x.Name.ToLower() == cityName.ToLower()).Include(c => c.Country).FirstOrDefaultAsync();
                if (dbCity != null)
                    result.Add(new LocateBaseResponseModel
                    {
                        Id = dbCity.Id,
                        Name = $"{dbCity.Name}, {dbCity.Country.Name}"
                    });
                else
                {
                    Country country = _repository.Filter<Country>(c => c.Name.ToLower() == variable.Terms[1].Value.ToLower()).FirstOrDefault();
                    if (country == null)
                    {
                        country = _repository.Create(new Country { Name = variable.Terms[1].Value });
                        _repository.SaveChanges();
                    }

                    City c = _repository.Create(new City
                    {
                        Country = country,
                        Name = cityName
                    });
                    _repository.SaveChanges();
                    result.Add(new LocateBaseResponseModel
                    {
                        Id = c.Id,
                        Name = $"{c.Name}, {c.Country.Name}"
                    });
                }
            }
            return result;
        }

        public IEnumerable<LocateBaseResponseModel> Cities(int countryId)
        {
            return _repository
                .Filter<City>(c => c.CountryId == countryId && !c.IsDeleted)
                .Select(s => new LocateBaseResponseModel
                {
                    Id = s.Id,
                    Name = s.Name
                });
        }

        public IEnumerable<LocateBaseResponseModel> Countries()
        {
            return _repository
                .Filter<Country>(c => !c.IsDeleted)
                .Select(s => new LocateBaseResponseModel
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .OrderBy(c => c.Name);
        }
    }
}