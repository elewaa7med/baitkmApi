using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.SaveFilters;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.SaveFilters
{
    public class SaveFilterService : ISaveFilterService
    {
        private readonly IEntityRepository _repository;
        private readonly IOptionsBinder _optionsBinder;
        public SaveFilterService(IEntityRepository repository,
            IOptionsBinder optionsBinder)
        {
            _repository = repository;
            _optionsBinder = optionsBinder;
        }

        public async Task<bool> Add(AddSaveFilterViewModel model, int userId, Language language, string deviceId)
        {
            SaveFilter saveFilter = new SaveFilter();
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                saveFilter.GuestId = guest.Id;
            }
            else
                saveFilter.UserId = user.Id;

            var filterCount = 0;
            if (string.IsNullOrEmpty(model.SaveFilterName))
                throw new Exception(_optionsBinder.Error().FilterName);
            if (!string.IsNullOrEmpty(model.Search))
            {
                saveFilter.Search = model.Search;
                filterCount += 1;
            }
            if (model.AnnouncementType.HasValue)
            {
                if (saveFilter.Description == null)
                    saveFilter.Description = model.AnnouncementType.ToString();
                saveFilter.AnnouncementType = model.AnnouncementType;
                filterCount += 1;
            }
            if (model.AnnouncementEstateType.HasValue)
            {
                if (saveFilter.Description == null)
                    saveFilter.Description = model.AnnouncementEstateType.ToString();
                saveFilter.AnnouncementEstateType = model.AnnouncementEstateType;
                filterCount += 1;
            }
            if (model.AnnouncementRentType.HasValue)
            {
                saveFilter.AnnouncementRentType = model.AnnouncementRentType;
                filterCount += 1;
            }
            if (model.AnnouncementResidentialType.HasValue)
            {
                saveFilter.AnnouncementResidentialType = model.AnnouncementResidentialType;
                filterCount += 1;
            }
            if (model.LandType.HasValue)
            {
                saveFilter.LandType = model.LandType;
                filterCount += 1;
            }
            if (model.ConstructionStatus.HasValue)
            {
                saveFilter.ConstructionStatus = model.ConstructionStatus;
                filterCount += 1;
            }
            if (model.LandCategory.HasValue)
            {
                saveFilter.LandCategory = model.LandCategory;
                filterCount += 1;
            }
            if (model.FurnishingStatus.HasValue)
            {
                saveFilter.FurnishingStatus = model.FurnishingStatus;
                filterCount += 1;
            }
            if (model.SaleType.HasValue)
            {
                saveFilter.SaleType = model.SaleType;
                filterCount += 1;
            }
            if (model.BuildingAge.HasValue)
            {
                saveFilter.BuildingAge = model.BuildingAge;
                filterCount += 1;
            }
            if (model.CommercialType.HasValue)
            {
                saveFilter.CommercialType = model.CommercialType;
                filterCount += 1;
            }
            if (model.FacadeType.HasValue)
            {
                saveFilter.FacadeType = model.FacadeType;
                filterCount += 1;
            }
            if (model.OwnerShip.HasValue)
            {
                saveFilter.OwnerShip = model.OwnerShip;
                filterCount += 1;
            }
            if (model.SittingCount.HasValue)
            {
                saveFilter.SittingCount = model.SittingCount;
                filterCount += 1;
            }
            if (model.CountryId.HasValue)
            {
                saveFilter.CountryId = model.CountryId;
                saveFilter.CountryName = model.CountryName;
                filterCount += 1;
            }
            if (model.CityId.HasValue)
            {
                saveFilter.CityId = model.CityId;
                saveFilter.CityName = model.CityName;
                filterCount += 1;
            }
            if (model.PriceFrom > 0 || model.PriceTo > 0)
            {
                saveFilter.PriceTo = model.PriceTo;
                saveFilter.PriceFrom = model.PriceFrom;
                filterCount += 1;
            }
            if (model.MinArea > 0 || model.MaxArea > 0)
            {
                saveFilter.MinArea = model.MinArea;
                saveFilter.MaxArea = model.MaxArea;
                filterCount += 1;
            }
            if (model.MaxMeterPrice.HasValue || model.MinMeterPrice.HasValue)
            {
                saveFilter.MinMeterPrice = model.MinMeterPrice;
                saveFilter.MaxMeterPrice = model.MaxMeterPrice;
                filterCount += 1;
            }
            if (model.BathroomCount != 0 && model.BathroomCount.HasValue)
            {
                saveFilter.BathroomCount = model.BathroomCount;
                filterCount += 1;
            }
            if (model.BedroomCount != 0 && model.BedroomCount.HasValue)
            {
                saveFilter.BedroomCount = model.BedroomCount;
                filterCount += 1;
            }
            saveFilter.SaveFilterName = model.SaveFilterName;

            if (model.Features.Count() != 0)
            {
                foreach (var item in model.Features)
                {
                    _repository.Create(new SaveFilterFeature
                    {
                        FeatureType = item,
                        SaveFilter = saveFilter
                    });
                }
                filterCount += 1;
            }
            saveFilter.FilterCount = filterCount;

            await _repository.CreateAsync(saveFilter);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id, int userId, Language language, string deviceId)
        {
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
            }

            if (user != null)
            {
                var saveFilter = _repository.GetById<SaveFilter>(id);
                if (saveFilter == null)
                    throw new Exception("saveFilter not found");
                var features = await _repository.Filter<SaveFilterFeature>(x =>
                    x.SaveFilterId == saveFilter.Id).ToListAsync();
                _repository.HardDeleteRange(features);
                _repository.HardDelete(saveFilter);
            }
            else
            {
                var saveFilter = _repository.GetById<SaveFilter>(id);
                if (saveFilter == null)
                    throw new Exception("saveFilter not found");
                var features = await _repository
                    .Filter<SaveFilterFeature>(x => x.SaveFilterId == saveFilter.Id).ToListAsync();
                _repository.HardDeleteRange(features);
                _repository.HardDelete(saveFilter);
            }
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Edit(UpdateSaveFilterModel model, int userId, int id, Language language, string deviceId)
        {
            SaveFilter saveFilter = null;
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                saveFilter = await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.Id == id).FirstOrDefaultAsync();
                if (saveFilter.GuestId != guest.Id)
                    throw new Exception(_optionsBinder.Error().NotParticipating);
            }
            else
            {
                saveFilter = await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.Id == id).FirstOrDefaultAsync();
                if (saveFilter.UserId != user.Id)
                    throw new Exception(_optionsBinder.Error().NotParticipating);
            }
            var filterCount = 0;
            if (string.IsNullOrEmpty(model.SaveFilterName))
                throw new Exception(_optionsBinder.Error().FilterName);
            if (!string.IsNullOrEmpty(model.Search))
            {
                saveFilter.Search = model.Search;
                filterCount += 1;
            }
            if (model.AnnouncementType.HasValue)
            {
                if (saveFilter.Description == null)
                    saveFilter.Description = model.AnnouncementType.ToString();
                saveFilter.AnnouncementType = model.AnnouncementType;
                filterCount += 1;
            }
            if (model.AnnouncementEstateType.HasValue)
            {
                if (saveFilter.Description == null)
                    saveFilter.Description = model.AnnouncementEstateType.ToString();
                saveFilter.AnnouncementEstateType = model.AnnouncementEstateType;
                filterCount += 1;
            }
            if (model.AnnouncementRentType.HasValue)
            {
                saveFilter.AnnouncementRentType = model.AnnouncementRentType;
                filterCount += 1;
            }
            if (model.AnnouncementResidentialType.HasValue)
            {
                saveFilter.AnnouncementResidentialType = model.AnnouncementResidentialType;
                filterCount += 1;
            }
            if (model.LandType.HasValue)
            {
                saveFilter.LandType = model.LandType;
                filterCount += 1;
            }
            if (model.ConstructionStatus.HasValue)
            {
                saveFilter.ConstructionStatus = model.ConstructionStatus;
                filterCount += 1;
            }
            if (model.LandCategory.HasValue)
            {
                saveFilter.LandCategory = model.LandCategory;
                filterCount += 1;
            }
            if (model.FurnishingStatus.HasValue)
            {
                saveFilter.FurnishingStatus = model.FurnishingStatus;
                filterCount += 1;
            }
            if (model.SaleType.HasValue)
            {
                saveFilter.SaleType = model.SaleType;
                filterCount += 1;
            }
            if (model.BuildingAge.HasValue)
            {
                saveFilter.BuildingAge = model.BuildingAge;
                filterCount += 1;
            }
            if (model.CommercialType.HasValue)
            {
                saveFilter.CommercialType = model.CommercialType;
                filterCount += 1;
            }
            if (model.FacadeType.HasValue)
            {
                saveFilter.FacadeType = model.FacadeType;
                filterCount += 1;
            }
            if (model.OwnerShip.HasValue)
            {
                saveFilter.OwnerShip = model.OwnerShip;
                filterCount += 1;
            }
            if (model.SittingCount.HasValue)
            {
                saveFilter.SittingCount = model.SittingCount;
                filterCount += 1;
            }
            if (model.CountryId.HasValue)
            {
                saveFilter.CountryId = model.CountryId;
                saveFilter.CountryName = model.CountryName;
                filterCount += 1;
            }
            if (model.CityId.HasValue)
            {
                saveFilter.CityId = model.CityId;
                saveFilter.CityName = model.CountryName;
                filterCount += 1;
            }
            if (model.PriceFrom > 0 || model.PriceTo > 0)
            {
                saveFilter.PriceTo = model.PriceTo;
                saveFilter.PriceFrom = model.PriceFrom;
                filterCount += 1;
            }
            if (model.MinArea > 0 || model.MaxArea > 0)
            {
                saveFilter.MinArea = model.MinArea;
                saveFilter.MaxArea = model.MaxArea;
                filterCount += 1;
            }
            if (model.MaxMeterPrice.HasValue || model.MinMeterPrice.HasValue)
            {
                saveFilter.MinMeterPrice = model.MinMeterPrice;
                saveFilter.MaxMeterPrice = model.MaxMeterPrice;
                filterCount += 1;
            }
            if (model.BathroomCount != 0 && model.BathroomCount.HasValue)
            {
                saveFilter.BathroomCount = model.BathroomCount;
                filterCount += 1;
            }
            if (model.BedroomCount != 0 && model.BedroomCount.HasValue)
            {
                saveFilter.BedroomCount = model.BedroomCount;
                filterCount += 1;
            }
            saveFilter.SaveFilterName = model.SaveFilterName;

            var features = await _repository
                .Filter<SaveFilterFeature>(x => x.SaveFilterId == saveFilter.Id).ToListAsync();
            _repository.HardDeleteRange(features);
            if (model.Features.Count() != 0)
            {
                foreach (var item in model.Features)
                {
                    _repository.Create(new SaveFilterFeature
                    {
                        FeatureType = item,
                        SaveFilterId = saveFilter.Id
                    });
                }
                filterCount += 1;
            }
            _repository.Update(saveFilter);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<SaveFilterViewModel> SaveFilterById(int id, int userId, Language language, string deviceId)
        {
            SaveFilter saveFilter = null;
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                saveFilter = await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.Id == id).FirstOrDefaultAsync();
                if (saveFilter.GuestId != guest.Id)
                    throw new Exception(_optionsBinder.Error().NotParticipating);
            }
            else
            {
                saveFilter = await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.Id == id).FirstOrDefaultAsync();
                if (saveFilter.UserId != user.Id)
                    throw new Exception(_optionsBinder.Error().NotParticipating);
            }
            return await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.Id == id)
                .Select(sf => new SaveFilterViewModel
                {
                    Id = sf.Id,
                    SaveFilterName = sf.SaveFilterName,
                    Search = sf.Search,
                    AnnouncementType = sf.AnnouncementType,
                    AnnouncementEstateType = sf.AnnouncementEstateType,
                    AnnouncementRentType = sf.AnnouncementRentType,
                    AnnouncementResidentialType = sf.AnnouncementResidentialType,
                    CityName = sf.CityName,
                    CountryName = sf.CountryName,
                    FurnishingStatus = sf.FurnishingStatus,
                    LandCategory = sf.LandCategory,
                    Features = sf.Features.Select(f => f.FeatureType).ToList(),
                    BathroomCount = sf.BathroomCount,
                    BedroomCount = sf.BedroomCount,
                    PriceFrom = sf.PriceFrom,
                    PriceTo = sf.PriceTo,
                    MinArea = sf.MinArea,
                    MaxArea = sf.MaxArea,
                    FilterCount = sf.FilterCount,
                    CityId = sf.CityId,
                    CountryId = sf.CountryId,
                    SittingCount = sf.SittingCount,
                    SaleType = sf.SaleType,
                    BuildingAge = sf.BuildingAge,
                    ConstructionStatus = sf.ConstructionStatus,
                    OwnerShip = sf.OwnerShip,
                    CommercialType = sf.CommercialType,
                    LandType = sf.LandType,
                    MaxMeterPrice = sf.MaxMeterPrice,
                    MinMeterPrice = sf.MinMeterPrice,
                    FacadeType = sf.FacadeType
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SaveFilterViewModel>> SaveFilterList(int userId, Language language, string deviceId)
        {
            IEnumerable<SaveFilter> saveFilters = null;
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                saveFilters = await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.GuestId == guest.Id)
                    .Include(sf => sf.Features).ToListAsync();
            }
            else
                saveFilters = await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.UserId == user.Id)
                    .Include(sf => sf.Features).ToListAsync();

            return saveFilters.OrderByDescending(sf => sf.CreatedDt)
                .Select(sf => new SaveFilterViewModel
                {
                    Id = sf.Id,
                    SaveFilterName = sf.SaveFilterName,
                    Search = sf.Search,
                    AnnouncementType = sf.AnnouncementType,
                    AnnouncementEstateType = sf.AnnouncementEstateType,
                    AnnouncementRentType = sf.AnnouncementRentType,
                    AnnouncementResidentialType = sf.AnnouncementResidentialType,
                    CityName = sf.CityName,
                    CountryName = sf.CountryName,
                    FurnishingStatus = sf.FurnishingStatus,
                    LandCategory = sf.LandCategory,
                    Features = sf.Features.Select(f => f.FeatureType).ToList(),
                    BathroomCount = sf.BathroomCount,
                    BedroomCount = sf.BedroomCount,
                    PriceFrom = sf.PriceFrom,
                    PriceTo = sf.PriceTo,
                    MinArea = sf.MinArea,
                    MaxArea = sf.MaxArea,
                    FilterCount = sf.FilterCount,
                    CityId = sf.CityId,
                    CountryId = sf.CountryId,
                    SittingCount = sf.SittingCount,
                    SaleType = sf.SaleType,
                    BuildingAge = sf.BuildingAge,
                    ConstructionStatus = sf.ConstructionStatus,
                    OwnerShip = sf.OwnerShip,
                    CommercialType = sf.CommercialType,
                    LandType = sf.LandType,
                    MaxMeterPrice = sf.MaxMeterPrice,
                    MinMeterPrice = sf.MinMeterPrice,
                    FacadeType = sf.FacadeType,
                });
        }
    }
}