using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Location;
using Baitkm.Entities;
using Baitkm.Enums.UserRelated;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.AnnouncementLocation;
using Baitkm.Infrastructure.Validation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.IpAddress
{
    public class LocateByIpAddress : ILocateByIpAddress
    {
        private readonly IEntityRepository _repository;
        private const string InvalidChars = "\"";
        public LocateByIpAddress(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task Locate(string userName, string ip, string verifiedBy)
        {
            var result = new LocateModel();
            if (string.IsNullOrEmpty(ip))
                return;
            Enum.TryParse(verifiedBy, out VerifiedBy verified);
            var caller = await _repository.Filter<User>(x => (x.Email == userName
                || (x.PhoneCode + x.Phone) == userName) && x.VerifiedBy == verified).FirstOrDefaultAsync();
            if (caller == null)
                throw new SmartException("No user found");
            dynamic property = new ExpandoObject();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConstValues.IpAddressUrl);
                var response = await client.GetAsync(ip);
                if (!response.IsSuccessStatusCode)
                    return;
                var serialized = await response.Content.ReadAsStringAsync();
                var token = JToken.Parse(serialized);
                var resultJson = token.SelectToken("data").ToString();
                result = JsonConvert.DeserializeObject<LocateModel>(resultJson);
            }
            if (result.Lat != 0.0m && result.Lng != 0.0m && Utilities.IsNullOrEmpty(result.City, result.Country))
            {
                if (string.IsNullOrEmpty(result.Address))
                    result.Address = "Unnamed Road";
                var userLocation = await _repository.Filter<UserLocation>(x => x.UserId == caller.Id).FirstOrDefaultAsync();
                lock (this)
                {
                    if (userLocation == null)
                    {
                        _repository.Create(new UserLocation
                        {
                            UserId = caller.Id,
                            Address = result.Address,
                            City = result.City,
                            Lat = result.Lat,
                            Long = result.Lng,
                            Country = result.Country,
                            IpAddress = result.Ip
                        });
                        _repository.SaveChanges();
                    }
                    else
                    {
                        userLocation.Address = result.Address;
                        userLocation.City = result.City;
                        userLocation.Lat = result.Lat;
                        userLocation.Long = result.Lng;
                        userLocation.Country = result.Country;
                        userLocation.IpAddress = result.Ip;
                        _repository.Update(userLocation);
                        _repository.SaveChanges();
                    }
                }
            }
        }

        #region
        private async Task<LocateModel> Locate(LocateModel model)
        {
            model.Country = await model.Country.CountryNameCheck(model.City);
            if (string.IsNullOrEmpty(model.Country))
                return new LocateModel();
            var addressUri = new Uri($"https://maps.googleapis.com/maps/api/geocode/json?latlng={model.Lat},{model.Lng}{ConstValues.GoogleLocateKey}");
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(addressUri);
                var json = await resp.Content.ReadAsStringAsync();
                var token = JToken.Parse(json);
                var array = token.SelectToken("results");
                if (!array.HasValues || !array.Any())
                    return model;
                var deserialized = JsonConvert.DeserializeObject<List<UserLocationParsingBase>>(array.ToString());
                deserialized.RemoveAll(x => x.Geometry.LocationType != "GEOMETRIC_CENTER");
                var item = deserialized.FirstOrDefault();
                if (item == null)
                    return model;
                var addressItem = item.AddressComponents.FirstOrDefault(x => x.Types.Contains("route"))?.Name;
                model.Address = addressItem;
            }
            model.Address = model.Address?.Replace(InvalidChars, string.Empty);
            return model;
        }
        #endregion
    }
}