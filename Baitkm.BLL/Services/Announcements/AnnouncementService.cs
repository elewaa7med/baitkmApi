using Baitkm.BLL.Helpers.Socket.AnnouncementProgress;
using Baitkm.BLL.Services.Activities;
using Baitkm.BLL.Services.Exchanges;
using Baitkm.BLL.Services.Scheduler.Jobs;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DAL.Repository.Firebase;
using Baitkm.DAL.Services;
using Baitkm.DTO.ViewModels.Admin;
using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Notifications;
using Baitkm.DTO.ViewModels.Persons.Users.CommonModels;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.Subscriptions;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.AnnouncementLocation;
using Baitkm.Infrastructure.Helpers.Binders;
using Baitkm.Infrastructure.Helpers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using AnnouncementLocateModel = Baitkm.Infrastructure.Helpers.AnnouncementLocation.AnnouncementLocateModel;

namespace Baitkm.BLL.Services.Announcements
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IEntityRepository _repository;
        private readonly MediaAccessor _accessor;
        private readonly IFirebaseRepository _firebaseRepository;
        private readonly IOptionsBinder _optionsBinder;
        private readonly IActivityService _activityService;
        private readonly IExchangeService _exchangeService;

        public AnnouncementService(IEntityRepository repository,
            IFirebaseRepository firebaseRepository,
            IOptionsBinder optionsBinder,
            IActivityService activityService,
            IExchangeService exchangeService
            )
        {
            _repository = repository;
            _accessor = new MediaAccessor();
            _firebaseRepository = firebaseRepository;
            _optionsBinder = optionsBinder;
            _activityService = activityService;
            _exchangeService = exchangeService;
        }

        public async Task<int> AddAsync(AddAnnouncementModel model, int userId, Language language)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            Announcement announcement;
            if (model.Area == 0.0m || string.IsNullOrEmpty(model.Area.ToString()))
                throw new Exception(_optionsBinder.Error().FillArea);
            if (model.Price == 0 || string.IsNullOrEmpty(model.Price.ToString()))
                throw new Exception(_optionsBinder.Error().FillPrice);
            if (model.CurrencyId < 1)
                throw new Exception("Invalid currency Id!");

            var result = await Utilities.GetAddressFromGoogle(model.Lat, model.Lng, Language.English);
            var resultArabian = await Utilities.GetAddressFromGoogle(model.Lat, model.Lng, Language.Arabian);
            var addressEn = Utilities.StringBuilderModel(result);
            var addressAr = Utilities.StringBuilderModel(resultArabian);
            //TODO - solve problem
            //if (result.Country == null && result.City == null)
            //    throw new Exception("Invalid Location!");
            Country country = _repository.Filter<Country>(c => c.Name == result.Country).FirstOrDefault();
            if (country == null && result.Country != null)
            {
                country = _repository.Create(new Country
                {
                    Name = result.Country
                });
                _repository.SaveChanges();
            }

            City city = _repository.Filter<City>(c => c.Name == result.City).FirstOrDefault();
            if (city == null && result.City != null)
            {
                city = _repository.Create(new City
                {
                    Country = country,
                    Name = result.City
                });
                _repository.SaveChanges();
            }
            decimal price;
            Currency currency = await _repository.Filter<Currency>(c => c.Id == model.CurrencyId).FirstOrDefaultAsync();
            if (model.CurrencyId == 1)
                price = model.Price;
            else
            {
                decimal currentRate = _repository.Filter<Rate>(r => r.CurrencyId == model.CurrencyId).FirstOrDefault().CurrentRate;
                price = Math.Round(model.Price / currentRate, 2);
            }
            announcement = new Announcement
            {
                AnnouncementType = model.AnnouncementType,
                AnnouncementEstateType = model.AnnouncementEstateType,
                Price = price,
                Area = model.Area,
                BathroomCount = model.BathroomCount ?? 0,
                BedroomCount = model.BedroomCount ?? 0,
                UserId = user.Id,
                AddressEn = addressEn,
                AddressAr = addressAr,
                CountryId = country.Id,
                CityId = city != null ? city.Id : 0,
                Lat = model.Lat,
                Lng = model.Lng,
                AnnouncementStatus = AnnouncementStatus.Accepted,
                AnnouncementStatusLastDay = DateTime.UtcNow,
                Title = model.Title,
                Description = model.Description,
                TitleArabian = model.TitleArabian,
                DescriptionArabian = model.DescriptionArabian,
                IsDraft = false,
                BalconyArea = model.BalconyArea,
                KitchenArea = model.KitchenArea,
                LaundryArea = model.LaundryArea,
                LivingArea = model.LivingArea,
                SittingCount = model.SittingCount,
                Floor = model.Floor ?? default,
                ConstructionStatus = model.ConstructionStatus,
                SaleType = model.SaleType,
                FurnishingStatus = model.FurnishingStatus,
                OwnerShip = model.OwnerShip,
                BuildingAge = model.BuildingAge,
                AnnouncementResidentialType = model.AnnouncementResidentialType,
                CurrencyId = 1,
                CommercialType = model.CommercialType,
                LandType = model.LandType,
                NumberOfAppartment = model.NumberOfAppartment,
                NumberOfFloors = model.NumberOfFloors,
                NumberOfVilla = model.NumberOfVilla,
                NumberOfShop = model.NumberOfShop,
                FireSystem = model.FireSystem,
                OfficeSpace = model.OfficeSpace,
                LaborResidence = model.LaborResidence,
                District = model.District,
                NumberOfWareHouse = model.NumberOfWareHouse,
                NumberOfUnits = model.NumberOfUnits,
            };
            if (model.AnnouncementType == AnnouncementType.Rent)
            {
                announcement.AnnouncementRentType = model.AnnouncementRentType;
                //announcement.IsOtherPeriod = model.IsOtherPeriod; //check
                if (model.AnnouncementRentType == null)
                    throw new Exception(_optionsBinder.Error().FillRentType);
            }
            _repository.Create(announcement);
            //Check
            //announcement.Address = $"{result.BuildingNumber} {result.StreetName} {result.Borough}";
            if (model.Features.Any())
            {
                foreach (var item in model.Features)
                {
                    _repository.Create(new AnnouncementFeature
                    {
                        FeatureType = item,
                        AnnouncementId = announcement.Id
                    });
                }
            }
            await _repository.SaveChangesAsync();
            var emails = await _repository.Filter<Email>(x => !x.IsDeleted).ToListAsync();
            foreach (var e in emails)
            {
                Utilities.SendKeyByEmail(e.EmailText, "Best regards Baitkm.", "Baitkm", $"We want to tell You that {user.FullName} has added a new announcement.");
            }
            //await UploadMap(model.Lat, model.Lng, announcement.Id);
            await SendSubscribersEmail(announcement);
            return announcement.Id;
        }

        //TO DO - add filter with location after check how to work city logic
        private async Task SendSubscribersEmail(Announcement announcement)
        {
            var subscribersEmail = await _repository.FilterAsNoTracking<SubscribeAnnouncement>(s =>
                s.AnnouncementType == announcement.AnnouncementType
                && s.AnnouncementEstateType == announcement.AnnouncementEstateType).ToListAsync();
            var html = Utilities.SubscribersEmailStyle(announcement.Id, announcement.AddressEn, announcement.CreatedDt, announcement.BedroomCount,
                announcement.BathroomCount, announcement.Rating, announcement.Area, announcement.Price);
            foreach (var item in subscribersEmail)
            {
                Utilities.SendEmail(item.Email, "Baitkm", html);
            }
        }

        public async Task<bool> EditAsync(EditAnnouncementModel model, int userId, Language language, int announcementId)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var announcement = await _repository.FilterAsNoTracking<Announcement>(a => a.Id == announcementId
                && a.UserId == user.Id).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            if (model.Area == 0.0m || string.IsNullOrEmpty(model.Area.ToString()))
                throw new Exception(_optionsBinder.Error().FillArea);
            if (model.Price == 0 || string.IsNullOrEmpty(model.Price.ToString()))
                throw new Exception(_optionsBinder.Error().FillPrice);

            var announcementFeatures = await _repository.Filter<AnnouncementFeature>(af =>
                af.AnnouncementId == announcementId).ToListAsync();
            _repository.HardDeleteRange<AnnouncementFeature>(announcementFeatures);
            foreach (var item in model.Features)
            {
                _repository.Create(new AnnouncementFeature
                {
                    FeatureType = item,
                    AnnouncementId = announcement.Id,
                });
            }
            if (model.Lat > 0 && model.Lng > 0)
            {
                var result = await Utilities.GetAddressFromGoogle(model.Lat.Value, model.Lng.Value, Language.English);
                var resultArabian = await Utilities.GetAddressFromGoogle(model.Lat.Value, model.Lng.Value, Language.Arabian);
                var addressEn = Utilities.StringBuilderModel(result);
                var addressAr = Utilities.StringBuilderModel(resultArabian);

                announcement.AddressEn = addressEn;
                announcement.AddressAr = addressAr;
                announcement.Lat = model.Lat.Value;
                announcement.Lng = model.Lng.Value;
                var announcementPhoto = await _repository.Filter<Attachment>(aa => aa.AnnouncementId == announcement.Id
                    && aa.AttachmentType == AttachmentType.OtherMapPhoto).FirstOrDefaultAsync();

                string mapPath = await _accessor.DownloadMap(new DownloadMapModel
                {
                    AnnouncementId = announcement.Id,
                    IsRelativeRequested = false,
                    Url =
                       $"{ConstValues.GoogleMapsBase}{model.Lat},{model.Lng}&markers=color:red%7C{model.Lat},{model.Lng}{ConstValues.GoogleLocateKey}"
                });
                if (announcementPhoto != null && mapPath != null)
                {
                    _repository.HardDelete(announcementPhoto);
                    await _accessor.Remove(announcementPhoto.File, UploadType.AnnouncementPhoto, announcement.Id);
                }
                else
                {
                    await _repository.CreateAsync(new Attachment
                    {
                        AnnouncementId = announcement.Id,
                        AttachmentType = AttachmentType.OtherMapPhoto,
                        File = mapPath
                    });
                }
            }

            decimal price;
            if (model.CurrencyId == 1)
                price = model.Price.Value;
            else
            {
                decimal currentRate = _repository.Filter<Rate>(r => r.CurrencyId == model.CurrencyId).FirstOrDefault().CurrentRate;
                price = Math.Round(model.Price.Value / currentRate, 2);
            }
            announcement.Price = price;
            announcement.IsOtherPeriod = model.IsOtherPeriod;
            announcement.Area = model.Area.Value;
            announcement.AnnouncementType = model.AnnouncementType.Value;
            announcement.AnnouncementResidentialType = model.AnnouncementResidentialType;
            announcement.LivingArea = model.LivingArea;
            announcement.SittingCount = model.SittingCount;
            announcement.KitchenArea = model.KitchenArea;
            announcement.BalconyArea = model.BalconyArea;
            announcement.LaundryArea = model.LaundryArea;
            announcement.ConstructionStatus = model.ConstructionStatus;
            announcement.SaleType = model.SaleType;
            announcement.FurnishingStatus = model.FurnishingStatus;
            announcement.AnnouncementEstateType = model.AnnouncementEstateType.Value;
            announcement.OwnerShip = model.OwnerShip;
            announcement.BuildingAge = model.BuildingAge;
            announcement.CommercialType = model.CommercialType;
            announcement.LandType = model.LandType;
            if (model.AnnouncementType == AnnouncementType.Rent)
                announcement.AnnouncementRentType = model.AnnouncementRentType;
            else
                announcement.AnnouncementRentType = null;
            announcement.BathroomCount = model.BathroomCount ?? 0;
            announcement.BedroomCount = model.BedroomCount ?? 0;
            //if (model.AnnouncementEstateType == AnnouncementEstateType.Land
            //    || model.AnnouncementResidentialType.Value != AnnouncementResidentialType.Building
            //    || model.AnnouncementResidentialType.Value != AnnouncementResidentialType.Tower
            //    || model.AnnouncementResidentialType.Value != AnnouncementResidentialType.Studio
            //    || model.AnnouncementEstateType != AnnouncementEstateType.Commercial)
            //{
            //    announcement.BathroomCount = 0;
            //    announcement.BedroomCount = 0;
            //}
            //else
            //{
            //    announcement.BathroomCount = model.BathroomCount;
            //    announcement.BedroomCount = model.BedroomCount;
            //}
            announcement.Title = model.Title;
            announcement.Description = model.Description;
            announcement.TitleArabian = model.TitleArabian;
            announcement.FacadeType = model.FacadeType;
            announcement.DescriptionArabian = model.DescriptionArabian;
            announcement.LandNumber = model.LandNumber;
            announcement.DisctrictName = model.DisctrictName;
            announcement.PlanNumber = model.PlanNumber;
            announcement.CurrencyId = model.CurrencyId.Value;
            announcement.StreetWidth = model.StreetWidth;
            announcement.NumberOfAppartment = model.NumberOfAppartment;
            announcement.NumberOfFloors = model.NumberOfFloors;
            announcement.NumberOfVilla = model.NumberOfVilla;
            announcement.NumberOfShop = model.NumberOfShop;
            announcement.FireSystem = model.FireSystem;
            announcement.OfficeSpace = model.OfficeSpace;
            announcement.LaborResidence = model.LaborResidence;
            announcement.District = model.District;
            announcement.NumberOfUnits = model.NumberOfUnits;
            announcement.LandCategory = model.LandCategory;
            announcement.NumberOfWareHouse = model.NumberOfWareHouse;
            _repository.Update(announcement);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadMap(decimal lat, decimal lng, int announcementId)
        {
            //var url = $"{ConstValues.GoogleMapsBase}{lat},{lng}&markers=color:red%7C{lat},{lng}{ConstValues.GoogleLocateKey}";
            //var mapPath = imageHandler.DownloadMap(announcementId, url, false);
            var mapPath = await _accessor.DownloadMap(new DownloadMapModel
            {
                AnnouncementId = announcementId,
                IsRelativeRequested = false,
                Url = $"{ConstValues.GoogleMapsBase}{lat},{lng}&markers=color:red%7C{lat},{lng}{ConstValues.GoogleLocateKey}"
            });
            _repository.Create(new Attachment
            {
                AnnouncementId = announcementId,
                AttachmentType = AttachmentType.OtherMapPhoto,
                UploadType = UploadType.AnnouncementMap,
                File = mapPath
            });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<ImageOptimizer> UploadBasePhoto(UploadFileModel model, int announcementId)
        {
            var announcement = await _repository.Filter<Announcement>(x => x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            if (!string.IsNullOrEmpty(announcement.BasePhoto))
                await _accessor.Remove(announcement.BasePhoto, UploadType.AnnouncementBasePhoto);
            var result = await _accessor.Upload(model.File, UploadType.AnnouncementBasePhoto);
            announcement.BasePhoto = Path.GetFileName(result);
            announcement.IsDraft = false;
            _repository.Update(announcement);
            await _repository.SaveChangesAsync();
            return new ImageOptimizer
            {
                Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.AnnouncementBasePhoto, result, ConstValues.Width, ConstValues.Height),
                PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.AnnouncementBasePhoto, result, 100, 100, true)
            };
        }

        public async Task<bool> UploadOtherPhotos(MultipleUpload model)
        {
            if (model.PhotoPaths == null)
                model.PhotoPaths = new List<string>();
            if (model.Photos == null)
                model.Photos = new List<IFormFile>();
            var announcement = await _repository.Filter<Announcement>(x => x.Id == model.AnnouncementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            var pathList = new List<string>();
            foreach (var variable in model.PhotoPaths)
            {
                if (string.IsNullOrEmpty(variable))
                    continue;
                var segments = new Uri(variable).Segments;
                if (segments.Length <= 5)
                    continue;
                var fileName = segments[5];
                fileName = fileName.Remove(fileName.Length - 1);
                pathList.Add(fileName);
            }
            var photos = await _repository.Filter<Attachment>(x =>
                    x.AnnouncementId == announcement.Id && x.AttachmentType == AttachmentType.OtherImages)
                .ToListAsync();
            foreach (var variable in photos)
            {
                var contains = pathList.Count(x => x == variable.File);
                if (contains != 0)
                    continue;
                await _accessor.Remove(variable.File, UploadType.AnnouncementPhoto, announcement.Id);
                _repository.HardDelete<Attachment>(variable);
            }
            var list = new List<List<IFormFile>>();
            for (var i = 0; i < model.Photos.Count; i += 2)
            {
                list.Add(model.Photos.GetRange(i, Math.Min(2, model.Photos.Count - i)));
            }
            Parallel.ForEach(list,
                async file =>
                {
                    try
                    {
                        await _accessor.MultipleUpload(file, UploadType.AnnouncementPhoto,
                            AttachmentType.OtherImages, announcement.Id);
                    }
                    catch (Exception)
                    {
                        ConstValues.ProgressModels.TryGetValue(model.AnnouncementId, out var existing);
                        if (existing != null)
                        {
                            existing.Total -= file.Count;
                            ConstValues.ProgressModels.TryRemove(model.AnnouncementId, out _);
                            if (existing.Total != existing.Done)
                                ConstValues.ProgressModels.TryAdd(model.AnnouncementId, existing);
                            var notify = Utilities.SerializeObject(new ProgressSocketEmitModel
                            {
                                AnnouncementId = model.AnnouncementId,
                                Increment = false
                            });
                            await AnnouncementProgressHandler.SendMessageAsync(announcement.UserId,
                                notify);
                        }
                    }
                });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadOtherDocumentation(MultipleUpload model)
        {
            if (model.PhotoPaths == null)
                model.PhotoPaths = new List<string>();
            if (model.Photos == null)
                model.Photos = new List<IFormFile>();
            var announcement = await _repository.Filter<Announcement>(x => x.Id == model.AnnouncementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            var pathList = new List<string>();
            foreach (var variable in model.PhotoPaths)
            {
                if (string.IsNullOrEmpty(variable))
                    continue;
                var segments = new Uri(variable).Segments;
                if (segments.Length <= 5)
                    continue;
                var fileName = segments[5];
                fileName = fileName.Remove(fileName.Length - 1);
                pathList.Add(fileName);
            }
            var photos = await _repository.Filter<Attachment>(x =>
                x.AnnouncementId == announcement.Id &&
                x.AttachmentType == AttachmentType.OtherDocumentations).ToListAsync();
            foreach (var variable in photos)
            {
                var contains = pathList.Count(x => x == variable.File);
                if (contains != 0)
                    continue;
                await _accessor.Remove(variable.File, UploadType.AnnouncementDocument, announcement.Id);
                _repository.HardDelete<Attachment>(variable);
            }
            var list = new List<List<IFormFile>>();
            for (var i = 0; i < model.Photos.Count; i += 2)
            {
                list.Add(model.Photos.GetRange(i, Math.Min(2, model.Photos.Count - i)));
            }
            Parallel.ForEach(list, async file =>
            {
                try
                {
                    await _accessor.MultipleUpload(file, UploadType.AnnouncementDocument,
                        AttachmentType.OtherDocumentations, announcement.Id);
                }
                catch (Exception)
                {
                    ConstValues.ProgressModels.TryGetValue(model.AnnouncementId, out var existing);
                    if (existing != null)
                    {
                        existing.Total -= file.Count;
                        ConstValues.ProgressModels.TryRemove(model.AnnouncementId, out _);
                        if (existing.Total != existing.Done)
                            ConstValues.ProgressModels.TryAdd(model.AnnouncementId, existing);
                        var notify = Utilities.SerializeObject(new ProgressSocketEmitModel
                        {
                            AnnouncementId = model.AnnouncementId,
                            Increment = false
                        });
                        await AnnouncementProgressHandler.SendMessageAsync(announcement.UserId,
                            notify);
                    }
                }
            });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadCallback(string path, int announcementId, AttachmentType announcementPhotoType)
        {
            ConstValues.ProgressModels.TryGetValue(announcementId, out var existing);
            var announcement = await _repository.Filter<Announcement>(x => x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            try
            {
                if (string.IsNullOrEmpty(path))
                    return true;
                _repository.Create(new Attachment
                {
                    AnnouncementId = announcement.Id,
                    AttachmentType = announcementPhotoType,
                    File = Path.GetFileName(path)
                });
                announcement.IsDraft = false;
                _repository.Update(announcement);
                await _repository.SaveChangesAsync();
                if (existing == null)
                    return true;
                ++existing.Done;
                ConstValues.ProgressModels.TryRemove(announcementId, out _);
                if (existing.Total != existing.Done)
                    ConstValues.ProgressModels.TryAdd(announcementId, existing);
                var notify = Utilities.SerializeObject(new ProgressSocketEmitModel
                {
                    AnnouncementId = announcementId,
                    Increment = true
                });
                await _repository.SaveChangesAsync();
                await AnnouncementProgressHandler.SendMessageAsync(announcement.UserId, notify);
                return true;
            }
            catch (Exception e)
            {
                if (existing != null)
                {
                    --existing.Total;
                    ConstValues.ProgressModels.TryRemove(announcementId, out _);
                    if (existing.Total != existing.Done)
                        ConstValues.ProgressModels.TryAdd(announcementId, existing);
                    var notify = Utilities.SerializeObject(new ProgressSocketEmitModel
                    {
                        AnnouncementId = announcementId,
                        Increment = false
                    });
                    await AnnouncementProgressHandler.SendMessageAsync(announcement.UserId,
                        notify);
                }
                throw new Exception(e.Message);
            }
        }

        public async Task<AnnouncementViewModel> GetMyAnnouncementDetails(int announcementId, int userId,
            Language language, string deviceId)
        {
            ViewedAnnouncement viewed = null;
            int userAnnouncementCount = 0;
            int connversationId;
            int? yourRating = 0;
            bool isSubscribe = false;
            bool isFavourite = false;
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();

            var announcement = _repository.GetById<Announcement>(announcementId);
            //TO DO fix validation
            //if (announcement.AnnouncementStatus != AnnouncementStatus.Accepted && guest != null || announcement.UserId != user.Id)
            //    throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            //if (announcement == null || announcement.AnnouncementStatus != AnnouncementStatus.Accepted || announcement.IsDraft)
            if ((user != null) && (user.RoleEnum != Role.Admin))
            {
                userAnnouncementCount = await _repository.Filter<Announcement>(x => !x.IsDraft && x.UserId == user.Id).CountAsync();
                viewed = await _repository.Filter<ViewedAnnouncement>(x =>
                    x.UserId == user.Id && x.AnnouncementId == announcement.Id).FirstOrDefaultAsync();
                if (viewed == null)
                {
                    _repository.Create(new ViewedAnnouncement
                    {
                        UserId = user.Id,
                        AnnouncementId = announcement.Id
                    });
                    announcement.ViewsCount++;
                }
                await _activityService.AddOrUpdate(announcement, user.Id, false, ActivityType.Watched);

                yourRating = await _repository.FilterAsNoTracking<Rating>(r => r.UserId == user.Id
                    && r.AnnouncementId == announcement.Id).Select(r => r.Rat).FirstOrDefaultAsync();
                if (yourRating == 0)
                    yourRating = null;
                isSubscribe = await _repository.FilterAsNoTracking<SubscribeAnnouncement>(s =>
                    s.UserId == user.Id && s.AnnouncementId == announcement.Id).AnyAsync();
                isFavourite = await _repository
                    .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == announcement.Id && x.UserId == user.Id).AnyAsync();
                var conversation = await _repository.Filter<Conversation>(x =>
                    x.AnnouncementCreatorId != user.Id && x.QuestionerId == user.Id
                    && x.AnnouncementId == announcement.Id).FirstOrDefaultAsync();
                connversationId = conversation?.Id ?? 0;
            }
            else if (guest != null)
            {
                viewed = await _repository.Filter<ViewedAnnouncement>(x =>
                    x.GuestId == guest.Id && x.AnnouncementId == announcement.Id).FirstOrDefaultAsync();
                if (viewed == null)
                {
                    _repository.Create(new ViewedAnnouncement
                    {
                        GuestId = guest.Id,
                        AnnouncementId = announcement.Id
                    });
                    announcement.ViewsCount++;
                }
                await _activityService.AddOrUpdate(announcement, guest.Id, true, ActivityType.Watched);

                yourRating = await _repository.FilterAsNoTracking<Rating>(r => r.GuestId == guest.Id
                    && r.AnnouncementId == announcement.Id).Select(r => r.Rat).FirstOrDefaultAsync();
                if (yourRating == 0)
                    yourRating = null;
                isSubscribe = await _repository.FilterAsNoTracking<SubscribeAnnouncement>(s =>
                    s.GuestId == guest.Id && s.AnnouncementId == announcement.Id).AnyAsync();
                isFavourite = await _repository
                    .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == announcement.Id && x.GuestId == guest.Id).AnyAsync();
            }
            _repository.Update(announcement);
            var result = await _repository.FilterAsNoTracking<Announcement>(x => x.Id == announcementId)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    //UserProfilePhoto_V2 = ConstValues.Url + Constants.FileFolder + Constants.ProfilePhoto + x.User.ProfilePhoto,
                    //BasePhoto_V2 = ConstValues.Url + Constants.FileFolder + Constants.AnnouncementPhotoWaterMark + x.BasePhoto,
                    //Photos_V2 = x.Attachments.Where(at => !at.IsDeleted)
                    //    .Select(at => new ImageOptimizer_V2
                    //    {
                    //        Id = at.Id,
                    //        UploadType = at.UploadType,
                    //        Photo = ConstValues.Url + Constants.FileFolder + Constants.AnnouncementPhotoWaterMark + at.Photo
                    //    }) ?? null,
                    //Videos_V2 = x.Attachments.Where(at => !at.IsDeleted && at.UploadType == UploadType.AnnouncementVideo)
                    //    .Select(at => new VideoOptimizer_V2
                    //    {
                    //        Id = at.Id,
                    //        UploadType = at.UploadType,
                    //        Video = ConstValues.Url + Constants.FileFolder + Constants.AnnouncementVideo + at.Photo,
                    //        ThumbNail = ConstValues.Url + Constants.FileFolder + Constants.AnnouncementVideo + Path.GetFileNameWithoutExtension(at.Photo) + "_thumb.jpg"
                    //    }) ?? null,

                    UserProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, a.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, announcementId) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, announcementId) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, announcementId),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                            : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                            ConstValues.Width, ConstValues.Height, false, announcementId),
                    }).ToList(),
                    Documents = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherDocumentations).Select(h => new DocumentOptimizer
                    {
                        Document = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementDocument, h.File, false, announcementId),
                        DocumentImage = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementDocument, h.File, ConstValues.Width, ConstValues.Height, false, announcementId),
                    }).ToList(),
                    MapPhoto = a.Attachments.Where(y => y.File.Contains("announcementMap")).Select(h =>
                       Utilities.ReturnFilePath(UploadType.AnnouncementPhoto, h.File, false, announcementId)).FirstOrDefault(),
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementType = a.AnnouncementType,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    ViewCount = announcement.ViewsCount,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    LandCategory = a.LandCategory,
                    StreetWidth = a.StreetWidth,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    CreateDate = a.CreatedDt,
                    Lat = a.Lat,
                    Lng = a.Lng,
                    PlanNumber = a.PlanNumber,
                    NumberOfUnits = a.NumberOfUnits,
                    DisctrictName = a.DisctrictName,
                    Price = a.Price,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    UserPhone = a.User.PhoneCode + a.User.Phone,
                    OwnerShip = a.OwnerShip,
                    MeterPrice = a.MeterPrice,
                    BuildingAge = a.BuildingAge,
                    OwnerEmail = a.User.Email,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    LandNumber = a.LandNumber,
                    FacadeType = a.FacadeType,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    Subscriptions = a.User.PersonSettings.Where(s => !s.IsDeleted).Select(y => y.SubscriptionsType).ToList(),
                    Features = a.Features.Select(f => f.FeatureType).ToList(),
                    ConversationId = 0,
                    IsTop = a.TopAnnouncement,
                    AnnouncementStatus = a.AnnouncementStatus,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    Title = a.Title,
                    Description = a.Description,
                    TitleArabian = a.TitleArabian,
                    CurrencySymbol = currency.Symbol,
                    CurrencyCode = currency.Code,
                    DescriptionArabian = a.DescriptionArabian,
                    UserAnnouncementCount = userAnnouncementCount,
                    IsOtherPeriod = a.IsOtherPeriod,
                    CurrencyId = a.CurrencyId,
                    CityId = a.CityId,
                    Rating = a.Rating,
                    AnnouncementRejectInfos = a.PersonNotifications.Where(y => y.AnnouncementId == announcementId
                        && y.UserId == announcement.UserId)
                        .Select(h => new AnnouncementRejectInfo
                        {
                            Title = language == Language.English ? h.Notification.Title
                           : h.Notification.NotificationTranslate.Where(l => l.Language == language).Select(n => n.Title).FirstOrDefault(),
                            AnnouncementRejectReason = language == Language.English ? h.Notification.Description
                           : h.Notification.NotificationTranslate.Where(l => l.Language == language).Select(n => n.Description).FirstOrDefault(),
                            AnnouncementRejectDate = h.CreatedDt,
                        }).ToList(),
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    IsSubscribe = isSubscribe,
                    IsFavourite = isFavourite,
                    YourRating = yourRating
                }).FirstOrDefaultAsync();

            City city = _repository.Filter<City>(c => c.Id == result.CityId).Include(c => c.Country).FirstOrDefault();
            result.City = city?.Name;
            result.Country = city?.Country.Name;
            if (currency.Id != 1)
            {
                decimal currentRate = _repository.Filter<Rate>(r => r.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                result.Price *= currentRate;
                result.MeterPrice /= currentRate;
            }
            await _repository.SaveChangesAsync();
            return result;
        }

        public async Task<MyAnnouncementDetailsForEditModel> GetMyAnnouncementDetailsForEdit(
            int userId, int announcementId, Language language)
        {
            var user = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId)
                .Select(a => new MyAnnouncementDetailsForEditModel
                {
                    Id = a.Id,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementType = a.AnnouncementType,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    Lat = a.Lat,
                    FacadeType = a.FacadeType,
                    Lng = a.Lng,
                    Price = Convert.ToInt64(a.Price),
                    Features = a.Features.Select(f => f.FeatureType).ToList(),
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                             UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                             UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages)
                        .Select(h => new ImageAndVideoOptimizer
                        {
                            Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                             UploadType.AnnouncementPhoto, h.File, false, announcementId) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                             UploadType.AnnouncementPhoto, h.File, false, announcementId) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                             UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, announcementId),
                            ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                             UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                             : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                             ConstValues.Width, ConstValues.Height, false, announcementId),
                        }).ToList(),
                    Documents = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherDocumentations)
                        .Select(h => new DocumentOptimizer
                        {
                            Document = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                             UploadType.AnnouncementDocument, h.File, false, announcementId),
                            DocumentImage = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                             UploadType.AnnouncementDocument, h.File, ConstValues.Width, ConstValues.Height, false, announcementId),
                        }).ToList(),
                    AnnouncementStatus = a.AnnouncementStatus,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    Title = a.Title,
                    Description = a.Description,
                    TitleArabian = a.TitleArabian,
                    DescriptionArabian = a.DescriptionArabian,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    KitchenArea = a.KitchenArea,
                    BalconyArea = a.BalconyArea,
                    StreetWidth = a.StreetWidth,
                    LaundryArea = a.LaundryArea,
                    DisctrictName = a.DisctrictName,
                    LandNumber = a.LandNumber,
                    Floor = a.Floor,
                    PlanNumber = a.PlanNumber,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    OfficeSpace = a.OfficeSpace,
                    LandCategory = a.LandCategory,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    IsOtherPeriod = a.IsOtherPeriod,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    NumberOfUnits = a.NumberOfUnits,
                    CurrencyId = currency.Id,
                    CurrencySymbol = currency.Symbol,
                    CurrencyCode = currency.Code
                }).FirstOrDefaultAsync();
            if (currency.Id != 1)
            {
                decimal currentRate = _repository.Filter<Rate>(r => r.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                announcement.Price *= currentRate;
            }
            return announcement;
        }

        public async Task<PagingResponseModel<AnnouncementViewModel>> MyAnnouncementList(MyAnnouncementListModel model,
            int userId, Language language)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var myAnnouncement = _repository.Filter<Announcement>(x => !x.IsDraft && x.UserId == user.Id);
            var myAnnouncementCount = myAnnouncement.Count();
            Currency currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();

            if (model.AnnouncementStatus != null)
                myAnnouncement = myAnnouncement.Where(a => a.AnnouncementStatus == model.AnnouncementStatus);
            List<AnnouncementViewModel> result = null;
            if (myAnnouncement.Count() != 0)
            {
                 result = myAnnouncement.Skip((model.Page - 1) * model.Count).Take(model.Count)
                    .OrderByDescending(x => x.CreatedDt)
                    .Select(a => new AnnouncementViewModel
                    {
                        Id = a.Id,
                        AnnouncementType = a.AnnouncementType,
                        AnnouncementEstateType = a.AnnouncementEstateType,
                        AnnouncementRentType = a.AnnouncementRentType,
                        AnnouncementStatus = a.AnnouncementStatus,
                        AnnouncementResidentialType = a.AnnouncementResidentialType,
                        Area = Convert.ToInt64(a.Area),
                        BathroomCount = a.BathroomCount,
                        BedroomCount = a.BedroomCount,
                        BalconyArea = a.BalconyArea,
                        KitchenArea = a.KitchenArea,
                        LaundryArea = a.LaundryArea,
                        LivingArea = a.LivingArea,
                        SittingCount = a.SittingCount,
                        Floor = a.Floor,
                        ConstructionStatus = a.ConstructionStatus,
                        SaleType = a.SaleType,
                        FurnishingStatus = a.FurnishingStatus,
                        OwnerShip = a.OwnerShip,
                        LandNumber = a.LandNumber,
                        StreetWidth = a.StreetWidth,
                        MeterPrice = a.MeterPrice,
                        BuildingAge = a.BuildingAge,
                        CurrencyId = a.CurrencyId,
                        Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                        Title = a.Title,
                        LandCategory = a.LandCategory,
                        DisctrictName = a.DisctrictName,
                        PlanNumber = a.PlanNumber,
                        Price = a.Price,
                        CurrencySymbol = currency.Symbol,
                        CurrencyCode = currency.Code,
                        CommercialType = a.CommercialType,
                        OfficeSpace = a.OfficeSpace,
                        LaborResidence = a.LaborResidence,
                        District = a.District,
                        NumberOfWareHouse = a.NumberOfWareHouse,
                        FacadeType = a.FacadeType,
                        LandType = a.LandType,
                        NumberOfAppartment = a.NumberOfAppartment,
                        NumberOfFloors = a.NumberOfFloors,
                        NumberOfVilla = a.NumberOfVilla,
                        NumberOfUnits = a.NumberOfUnits,
                        NumberOfShop = a.NumberOfShop,
                        FireSystem = a.FireSystem,
                        IsFavourite = a.Favourites.Select(y => y.UserId == user.Id).FirstOrDefault(),
                        ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                        MediasCount = a.Attachments.Count,
                        CityId = a.CityId,
                        Photo = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                        },
                        Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                        {
                            Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                         UploadType.AnnouncementPhoto, h.File, false, a.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                         UploadType.AnnouncementPhoto, h.File, false, a.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                         UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, a.Id),
                            ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                         UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                         : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                         ConstValues.Width, ConstValues.Height, false, a.Id),
                        }).ToList(),
                        CreateDate = a.CreatedDt,
                        Rating = a.Rating
                    }).ToList();
                foreach (var r in result)
                {
                    City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                    r.City = city?.Name;
                    r.Country = city?.Country.Name;
                    if (currency.Id != 0 && currency.Id != 1)
                    {
                        decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                        r.Price *= currentRate;
                        r.MeterPrice /= currentRate;
                    }
                }
            }
            return new PagingResponseModel<AnnouncementViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? myAnnouncement.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = myAnnouncementCount,
                PageCount = result == null ? 0 : Convert.ToInt32(Math.Ceiling(decimal.Divide(myAnnouncementCount, model.Count)))
            };
            
        }

        public async Task<PagingResponseModel<AnnouncementViewModel>> MyAnnouncementListMobile(MyAnnouncementListModel model,
           int userId, Language language)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var myAnnouncement = _repository.Filter<Announcement>(x => !x.IsDraft && x.UserId == user.Id);
            var myAnnouncementCount = myAnnouncement.Count();
            Currency currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();

            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(myAnnouncementCount, model.Count)));
            if (model.AnnouncementStatus != null)
                myAnnouncement = myAnnouncement.Where(a => a.AnnouncementStatus == model.AnnouncementStatus);

            var result = await myAnnouncement.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    MeterPrice = a.MeterPrice,
                    FurnishingStatus = a.FurnishingStatus,
                    NumberOfUnits = a.NumberOfUnits,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    FacadeType = a.FacadeType,
                    LandCategory = a.LandCategory,
                    CurrencyId = a.CurrencyId,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr.Trim(),
                    DisctrictName = a.DisctrictName,
                    Price = a.Price,
                    CommercialType = a.CommercialType,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    PlanNumber = a.PlanNumber,
                    StreetWidth = a.StreetWidth,
                    CurrencySymbol = currency.Symbol,
                    CurrencyCode = currency.Code,
                    LandNumber = a.LandNumber,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    Title = a.Title,
                    FireSystem = a.FireSystem,
                    LandType = a.LandType,
                    IsFavourite = a.Favourites.Select(y => y.UserId == user.Id).FirstOrDefault(),
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    MediasCount = a.Attachments.Count,
                    CityId = a.CityId,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                      UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                           UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    CreateDate = a.CreatedDt,
                    Rating = a.Rating
                }).ToListAsync();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }
            }

            return new PagingResponseModel<AnnouncementViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? myAnnouncement.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = myAnnouncementCount,
                PageCount = page
            };
        }

        public async Task<PagingResponseModel<AnnouncementViewModel>> MyAnnouncementListByUserId(PagingRequestModel model, int userId, Language language)
        {
            var myAnnouncement = _repository.Filter<Announcement>(x => !x.IsDraft && x.UserId == userId);
            if (!myAnnouncement.Any())
            {
                return new PagingResponseModel<AnnouncementViewModel>
                {
                    Data = new List<AnnouncementViewModel>(),
                    DateFrom = model.Count == 1 ? myAnnouncement.FirstOrDefault()?.CreatedDt : model.DateFrom,
                    ItemCount = 0,
                    PageCount = 0
                };
            }
            var user = _repository.Filter<User>(u => u.Id == userId).FirstOrDefault();
            Currency currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            var result = myAnnouncement.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    CurrencySymbol = currency.Symbol,
                    CurrencyCode = currency.Code,
                    PlanNumber = a.PlanNumber,
                    FacadeType = a.FacadeType,
                    Floor = a.Floor,
                    StreetWidth = a.StreetWidth,
                    ConstructionStatus = a.ConstructionStatus,
                    MeterPrice = a.MeterPrice,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    BedroomCount = a.BedroomCount,
                    OwnerShip = a.OwnerShip,
                    NumberOfUnits = a.NumberOfUnits,
                    NumberOfAppartment = a.NumberOfAppartment,
                    LandCategory = a.LandCategory,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    BuildingAge = a.BuildingAge,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    DisctrictName = a.DisctrictName,
                    LandNumber = a.LandNumber,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    LandType = a.LandType,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    Price = a.Price,
                    PublishDay = a.AnnouncementStatusLastDay.AddDays(-30),
                    RemainingDay = (a.AnnouncementStatusLastDay - DateTime.Today).Days > 0 ?
                        (a.AnnouncementStatusLastDay - DateTime.Today).Days : 0,
                    IsFavourite = a.Favourites.Select(y => y.UserId == userId).FirstOrDefault(),
                    CurrencyId = a.CurrencyId,
                    CreateDate = a.CreatedDt,
                    CityId = a.CityId,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                       UploadType.AnnouncementPhoto, h.File, false, a.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                       UploadType.AnnouncementPhoto, h.File, false, a.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                       UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, a.Id),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                       UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                       : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                       ConstValues.Width, ConstValues.Height, false, a.Id),
                    }).ToList(),
                    Rating = a.Rating
                }).ToList();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }
            }
            return new PagingResponseModel<AnnouncementViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? myAnnouncement.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = myAnnouncement.Count(),
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(myAnnouncement.Count(), model.Count)))
            };
        }

        public async Task<bool> FavouriteAsync(int announcementId, int userId, string deviceId, Language language)
        {
            bool isFavourite;
            var announcement = await _repository.FilterAsNoTracking<Announcement>(a => a.Id == announcementId
                && a.AnnouncementStatus != AnnouncementStatus.Rejected && !a.User.IsBlocked).FirstOrDefaultAsync();
            if (announcement == null || announcement.IsDraft)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);

            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            {
                isFavourite = await _repository.Filter<Favourite>(x => x.AnnouncementId == announcementId &&
                    x.UserId == user.Id).AnyAsync();
                if (!isFavourite)
                    _repository.Create(new Favourite
                    {
                        AnnouncementId = announcement.Id,
                        UserId = user.Id
                    });
                await _activityService.AddOrUpdate(announcement, user.Id, false, ActivityType.Favorite);
            }
            else
            {
                var guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                isFavourite = await _repository.Filter<Favourite>(x => x.AnnouncementId == announcementId &&
                    x.GuestId == guest.Id).AnyAsync();
                if (!isFavourite)
                    _repository.Create(new Favourite
                    {
                        AnnouncementId = announcement.Id,
                        GuestId = guest.Id
                    });
                await _activityService.AddOrUpdate(announcement, guest.Id, true, ActivityType.Favorite);
            }
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnFavouriteAsync(int announcementId, int userId, string deviceId, Language language)
        {
            var announcement = await _repository.FilterAsNoTracking<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                && x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);

            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            {
                var favourite = await _repository.Filter<Favourite>(x => x.AnnouncementId == announcementId
                    && x.UserId == user.Id).FirstOrDefaultAsync();
                _repository.HardDelete(favourite);
                await _activityService.AddOrUpdate(announcement, user.Id, false, ActivityType.UnFavorite);
            }
            else
            {
                var guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().NoGuest);
                var favourite = await _repository.Filter<Favourite>(x => x.AnnouncementId == announcementId
                    && x.GuestId == guest.Id).FirstOrDefaultAsync();
                _repository.HardDelete(favourite);
                await _activityService.AddOrUpdate(announcement, guest.Id, true, ActivityType.UnFavorite);
            }
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AnnouncementViewModel>> FavouriteListAsync(int userId, string deviceId, Language language,
            SortingType sortingType)
        {
            IEnumerable<AnnouncementViewModel> result = null;
            IQueryable<Favourite> query = null;
            Currency currency;
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            {
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
                query = _repository.FilterAsNoTracking<Favourite>(x => x.UserId == user.Id && !x.User.IsBlocked &&
                     x.Announcement.AnnouncementStatus == AnnouncementStatus.Accepted && !x.Announcement.IsDeleted);
            }
            else
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
                query = _repository.FilterAsNoTracking<Favourite>(x => x.GuestId == guest.Id
                    && x.Announcement.AnnouncementStatus != AnnouncementStatus.Hidden
                    && !x.Announcement.User.IsBlocked && !x.Announcement.IsDeleted);
            }
            result = query.OrderByDescending(f => f.CreatedDt)
                .Select(f => new AnnouncementViewModel
                {
                    Id = f.AnnouncementId,
                    Features = f.Announcement.Features.Where(a => !a.IsDeleted).Select(a => a.FeatureType).ToList(),
                    AnnouncementEstateType = f.Announcement.AnnouncementEstateType,
                    AnnouncementRentType = f.Announcement.AnnouncementRentType,
                    AnnouncementResidentialType = f.Announcement.AnnouncementResidentialType,
                    AnnouncementStatus = f.Announcement.AnnouncementStatus,
                    AnnouncementType = f.Announcement.AnnouncementType,
                    Area = Convert.ToInt64(f.Announcement.Area),
                    BathroomCount = f.Announcement.BathroomCount,
                    BalconyArea = f.Announcement.BalconyArea,
                    KitchenArea = f.Announcement.KitchenArea,
                    LaundryArea = f.Announcement.LaundryArea,
                    LivingArea = f.Announcement.LivingArea,
                    SittingCount = f.Announcement.SittingCount,
                    Floor = f.Announcement.Floor,
                    ConstructionStatus = f.Announcement.ConstructionStatus,
                    SaleType = f.Announcement.SaleType,
                    FurnishingStatus = f.Announcement.FurnishingStatus,
                    PlanNumber = f.Announcement.PlanNumber,
                    BedroomCount = f.Announcement.BedroomCount,
                    OwnerShip = f.Announcement.OwnerShip,
                    DisctrictName = f.Announcement.DisctrictName,
                    BuildingAge = f.Announcement.BuildingAge,
                    Price = f.Announcement.Price,
                    LandNumber = f.Announcement.LandNumber,
                    OfficeSpace = f.Announcement.OfficeSpace,
                    LaborResidence = f.Announcement.LaborResidence,
                    District = f.Announcement.District,
                    NumberOfWareHouse = f.Announcement.NumberOfWareHouse,
                    MeterPrice = f.Announcement.MeterPrice,
                    NumberOfUnits = f.Announcement.NumberOfUnits,
                    CommercialType = f.Announcement.CommercialType,
                    CurrencyCode = currency.Code,
                    CurrencySymbol = currency.Symbol,
                    NumberOfAppartment = f.Announcement.NumberOfAppartment,
                    NumberOfFloors = f.Announcement.NumberOfFloors,
                    NumberOfVilla = f.Announcement.NumberOfVilla,
                    NumberOfShop = f.Announcement.NumberOfShop,
                    FireSystem = f.Announcement.FireSystem,
                    LandType = f.Announcement.LandType,
                    FacadeType = f.Announcement.FacadeType,
                    CurrencyId = f.Announcement.CurrencyId,
                    LandCategory = f.Announcement.LandCategory,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                           UploadType.AnnouncementBasePhoto, f.Announcement.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                           UploadType.AnnouncementBasePhoto, f.Announcement.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = f.Announcement.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                        UploadType.AnnouncementPhoto, h.File, false, f.Announcement.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                        UploadType.AnnouncementPhoto, h.File, false, f.Announcement.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, f.Announcement.Id),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                        : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                        ConstValues.Width, ConstValues.Height, false, f.Announcement.Id),
                    }).ToList(),
                    IsFavourite = true,
                    Address = language == Language.English ? f.Announcement.AddressEn.Trim() : f.Announcement.AddressAr != null ? f.Announcement.AddressAr.Trim() : null,
                    Description = f.Announcement.Description,
                    StreetWidth = f.Announcement.StreetWidth,
                    Title = f.Announcement.Title,
                    CreatedDt = f.Announcement.CreatedDt,
                    CreateDate = f.CreatedDt,
                    ShareUrl = $"https://baitkm.com/products/details/{f.AnnouncementId}",
                    CityId = f.Announcement.CityId,
                    Rating = f.Announcement.Rating
                }).ToList();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }
            }

            switch (sortingType)
            {
                case SortingType.BedsLeast:
                    result = result.OrderByDescending(a => a.BedroomCount).ToList();
                    break;
                case SortingType.BedsMost:
                    result = result.OrderBy(a => a.BedroomCount).ToList();
                    break;
                case SortingType.Featured:
                    result = result.OrderBy(a => a.Features.Count());
                    break;
                case SortingType.Newest:
                    result = result.OrderBy(a => a.CreatedDt).ToList();
                    break;
                case SortingType.PriceHigh:
                    result = result.OrderByDescending(a => a.Price).ToList();
                    break;
                case SortingType.PriceLow:
                    result = result.OrderBy(a => a.Price).ToList();
                    break;
            }
            return result;
        }

        public async Task<PagingResponseModel<AnnouncementListViewModel>> FeaturedList(PagingRequestModel model,
            int userId, string deviceId, SortingType sortingType, Language language)
        {
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            var announcementFeatured = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked && x.TopAnnouncement
                && x.AnnouncementStatus == AnnouncementStatus.Accepted);

            //deviceId = "42741dfb7e9cf2c3a1db8a4e7b5b0f80";
            //var user = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            //var guest = await _repository.Filter<Guest>(x => x.DeviceId == deviceId).FirstOrDefaultAsync();
            //.Include(af => af.User).Include(af => af.Attachments).Include(af => af.Favourites).ToListAsync();
            //Currency currency;
            //if (user != null)
            //    currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            //else
            //    currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();

            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(announcementFeatured.Count(), model.Count)));

            var result = announcementFeatured
                .Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt)
                .Select(a => new AnnouncementListViewModel
                {
                    Id = a.Id,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    Area = Convert.ToInt64(a.Area),
                    Price = a.Price,
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    UserId = a.UserId,
                    BalconyArea = a.BalconyArea,
                    Title = a.Title,
                    TitleArabian = a.TitleArabian,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    UserName = a.User.FullName,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    PlanNumber = a.PlanNumber,
                    StreetWidth = a.StreetWidth,
                    LandNumber = a.LandNumber,
                    CommercialType = a.CommercialType,
                    CurrencyCode = currency.Code,
                    CurrencySymbol = currency.Symbol,
                    LandType = a.LandType,
                    NumberOfUnits = a.NumberOfUnits,
                    DisctrictName = a.DisctrictName,
                    LandCategory = a.LandCategory,
                    MeterPrice = a.MeterPrice,
                    CurrencyId = a.CurrencyId,
                    FacadeType = a.FacadeType,
                    CreatedDt = a.CreatedDt,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                       UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                       UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                          UploadType.AnnouncementPhoto, h.File, false, a.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                          UploadType.AnnouncementPhoto, h.File, false, a.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                          UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, a.Id),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                          UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                          : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                          ConstValues.Width, ConstValues.Height, false, a.Id),
                    }).ToList(),
                    CreateDate = a.CreatedDt,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    CityId = a.CityId,
                    Rating = a.Rating
                }).ToList();
            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }

                if (user != null)
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.UserId == user.Id).AnyAsync();
                else
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.GuestId == guest.Id).AnyAsync();
            }

            switch (sortingType)
            {
                case SortingType.BedsLeast:
                    result = result.OrderByDescending(a => a.BedroomCount).ToList();
                    break;
                case SortingType.BedsMost:
                    result = result.OrderBy(a => a.BedroomCount).ToList();
                    break;
                case SortingType.Featured:
                    break;
                case SortingType.Newest:
                    result = result.OrderByDescending(a => a.CreatedDt).ToList();
                    break;
                case SortingType.PriceHigh:
                    result = result.OrderByDescending(a => a.Price).ToList();
                    break;
                case SortingType.PriceLow:
                    result = result.OrderBy(a => a.Price).ToList();
                    break;
            }

            return new PagingResponseModel<AnnouncementListViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? announcementFeatured.FirstOrDefault().CreatedDt : model.DateFrom,
                ItemCount = announcementFeatured.Count(),
                PageCount = page
            };
        }

        public async Task<PagingResponseModel<AnnouncementListViewModel>> FeaturedListMobile(PagingRequestModel model,
           int userId, string deviceId, SortingType sortingType, Language language)
        {
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            var announcementFeatured = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked && x.TopAnnouncement
                && x.AnnouncementStatus == AnnouncementStatus.Accepted);

            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(announcementFeatured.Count(), model.Count)));

            var result = announcementFeatured
                .Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt)
                .Select(a => new AnnouncementListViewModel
                {
                    Id = a.Id,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    Area = Convert.ToInt64(a.Area),
                    Price = a.Price,
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    UserId = a.UserId,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    LandNumber = a.LandNumber,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    StreetWidth = a.StreetWidth,
                    MeterPrice = a.MeterPrice,
                    Title = a.Title,
                    UserName = a.User.FullName,
                    PlanNumber = a.PlanNumber,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    DisctrictName = a.DisctrictName,
                    CommercialType = a.CommercialType,
                    LandCategory = a.LandCategory,
                    CurrencySymbol = currency.Symbol,
                    CurrencyCode = currency.Code,
                    LandType = a.LandType,
                    FacadeType = a.FacadeType,
                    CurrencyId = a.CurrencyId,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    CreatedDt = a.CreatedDt,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfUnits = a.NumberOfUnits,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    CreateDate = a.CreatedDt,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    CityId = a.CityId,
                    Rating = a.Rating
                }).ToList();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }

                if (user != null)
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.UserId == user.Id).AnyAsync();
                else
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.GuestId == guest.Id).AnyAsync();
            }

            switch (sortingType)
            {
                case SortingType.BedsLeast:
                    result = result.OrderByDescending(a => a.BedroomCount).ToList();
                    break;
                case SortingType.BedsMost:
                    result = result.OrderBy(a => a.BedroomCount).ToList();
                    break;
                case SortingType.Featured:
                    break;
                case SortingType.Newest:
                    result = result.OrderByDescending(a => a.CreatedDt).ToList();
                    break;
                case SortingType.PriceHigh:
                    result = result.OrderByDescending(a => a.Price).ToList();
                    break;
                case SortingType.PriceLow:
                    result = result.OrderBy(a => a.Price).ToList();
                    break;
            }

            return new PagingResponseModel<AnnouncementListViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? announcementFeatured.FirstOrDefault().CreatedDt : model.DateFrom,
                ItemCount = announcementFeatured.Count(),
                PageCount = page
            };
        }

        public async Task<PagingResponseForSuggesting<AnnouncementViewModel>> SuggestingAsync(PagingRequestModel model,
           int userId, string deviceId, Language language)
        {
            List<SaveFilter> filters = new List<SaveFilter>();
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                filters = await _repository.Filter<SaveFilter>(sf => sf.GuestId == guest.Id).ToListAsync();
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
            {
                filters = await _repository.Filter<SaveFilter>(sf => sf.UserId == user.Id).ToListAsync();
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            }

            var query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                && x.AnnouncementStatus == AnnouncementStatus.Accepted);
            bool f = false;
            if (query.Count() == 0)
                return null;

            Expression<Func<Announcement, bool>> expr = default;
            foreach (var filter in filters)
            {
                f = true;
                Expression<Func<Announcement, bool>> baseExpr = default;
                if (filter.AnnouncementType != null)
                    baseExpr = x => x.AnnouncementType == filter.AnnouncementType;
                if (filter.BathroomCount != null && filter.BathroomCount != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.BathroomCount == filter.BathroomCount;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.BathroomCount == filter.BathroomCount;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.BedroomCount != null && filter.BedroomCount != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.BedroomCount == filter.BedroomCount;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.BedroomCount == filter.BedroomCount;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.PriceFrom != null && filter.PriceFrom != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Price >= filter.PriceFrom;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Price >= filter.PriceFrom;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.PriceTo != null && filter.PriceTo != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Price <= filter.PriceTo;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Price <= filter.PriceTo;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.MinArea != null && filter.MinArea != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Area >= filter.MinArea;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Area >= filter.MinArea;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.MaxArea != null && filter.MaxArea != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Area <= filter.MaxArea;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Area <= filter.MaxArea;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                //var features = await _repository
                //    .Filter<SaveFilterFeature>(x => x.SaveFilterId == filter.Id)
                //    .Select(x => x.FeatureType).ToListAsync();
                //Expression<Func<Announcement, bool>> tempEnumExpr = default;
                //foreach (var feature in features)
                //{
                //    if (tempEnumExpr == default)
                //        tempEnumExpr = a => a.Features.Count(x => x.FeatureType == feature) > 0;
                //    else
                //    {
                //        Expression<Func<Announcement, bool>> enumExpr = x =>
                //            x.Features.Count(s => s.FeatureType == feature) > 0;
                //        tempEnumExpr = tempEnumExpr.Or(enumExpr);
                //        baseExpr = baseExpr.And(tempEnumExpr);
                //    }
                //}
                if (expr == default)
                {
                    expr = baseExpr;
                }
                else
                {
                    if (baseExpr != default)
                        expr = expr.Or(baseExpr);
                }
            }
            if (expr != default)
                query = query.Where(expr);
            if (query.Count() < 10 && filters.Count != 0)
                query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                     && x.AnnouncementStatus == AnnouncementStatus.Accepted);

            var result = query.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    Price = a.Price,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FacadeType = a.FacadeType,
                    FurnishingStatus = a.FurnishingStatus,
                    Title = a.Title,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    LandCategory = a.LandCategory,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    CurrencyCode = currency.Code,
                    CurrencySymbol = currency.Symbol,
                    DisctrictName = a.DisctrictName,
                    Description = a.Description,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    OwnerShip = a.OwnerShip,
                    LandNumber = a.LandNumber,
                    StreetWidth = a.StreetWidth,
                    NumberOfUnits = a.NumberOfUnits,
                    PlanNumber = a.PlanNumber,
                    MeterPrice = a.MeterPrice,
                    BuildingAge = a.BuildingAge,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    CityId = a.CityId,
                    CurrencyId = a.CurrencyId,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, a.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, a.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, a.Id),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                            : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                            ConstValues.Width, ConstValues.Height, false, a.Id),
                    }).ToList(),
                    CreateDate = a.CreatedDt,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    Rating = a.Rating
                }).ToList();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }

                if (user != null)
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.UserId == user.Id).AnyAsync();
                else
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.GuestId == guest.Id).AnyAsync();
            }

            return new PagingResponseForSuggesting<AnnouncementViewModel>
            {
                ItemCount = query.Count(),
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(query.Count(), model.Count))),
                DateFrom = null,
                Data = result,
                IsSaveFilter = f
            };
        }

        public async Task<PagingResponseForSuggesting<AnnouncementViewModel>> SuggestingMobile(PagingRequestModel model,
            int userId, string deviceId, Language language)
        {
            List<SaveFilter> filters = new List<SaveFilter>();
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                filters = await _repository.Filter<SaveFilter>(sf => sf.GuestId == guest.Id).ToListAsync();
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
            {
                filters = await _repository.Filter<SaveFilter>(sf => sf.UserId == user.Id).ToListAsync();
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            }

            var query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                && x.AnnouncementStatus == AnnouncementStatus.Accepted);
            bool f = false;
            if (query.Count() == 0)
                return null;

            Expression<Func<Announcement, bool>> expr = default;
            foreach (var filter in filters)
            {
                f = true;
                Expression<Func<Announcement, bool>> baseExpr = default;
                if (filter.AnnouncementType != null)
                    baseExpr = x => x.AnnouncementType == filter.AnnouncementType;
                if (filter.BathroomCount != null && filter.BathroomCount != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.BathroomCount == filter.BathroomCount;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.BathroomCount == filter.BathroomCount;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.BedroomCount != null && filter.BedroomCount != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.BedroomCount == filter.BedroomCount;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.BedroomCount == filter.BedroomCount;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.PriceFrom != null && filter.PriceFrom != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Price >= filter.PriceFrom;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Price >= filter.PriceFrom;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.PriceTo != null && filter.PriceTo != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Price <= filter.PriceTo;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Price <= filter.PriceTo;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.MinArea != null && filter.MinArea != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Area >= filter.MinArea;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Area >= filter.MinArea;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                if (filter.MaxArea != null && filter.MaxArea != 0)
                {
                    if (baseExpr == default)
                        baseExpr = x => x.Area <= filter.MaxArea;
                    else
                    {
                        Expression<Func<Announcement, bool>>
                            tempExpr = x => x.Area <= filter.MaxArea;
                        baseExpr = baseExpr.And(tempExpr);
                    }
                }
                //var features = await _repository
                //    .Filter<SaveFilterFeature>(x => x.SaveFilterId == filter.Id)
                //    .Select(x => x.FeatureType).ToListAsync();
                //Expression<Func<Announcement, bool>> tempEnumExpr = default;
                //foreach (var feature in features)
                //{
                //    if (tempEnumExpr == default)
                //        tempEnumExpr = a => a.Features.Count(x => x.FeatureType == feature) > 0;
                //    else
                //    {
                //        Expression<Func<Announcement, bool>> enumExpr = x =>
                //            x.Features.Count(s => s.FeatureType == feature) > 0;
                //        tempEnumExpr = tempEnumExpr.Or(enumExpr);
                //        baseExpr = baseExpr.And(tempEnumExpr);
                //    }
                //}
                if (expr == default)
                {
                    expr = baseExpr;
                }
                else
                {
                    if (baseExpr != default)
                        expr = expr.Or(baseExpr);
                }
            }
            if (expr != default)
                query = query.Where(expr);
            if (query.Count() < 10 && filters.Count != 0)
                query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                     && x.AnnouncementStatus == AnnouncementStatus.Accepted);

            var result = query.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    Price = a.Price,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FacadeType = a.FacadeType,
                    FurnishingStatus = a.FurnishingStatus,
                    Title = a.Title,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    LandCategory = a.LandCategory,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    CurrencyCode = currency.Code,
                    CurrencySymbol = currency.Symbol,
                    DisctrictName = a.DisctrictName,
                    Description = a.Description,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    OwnerShip = a.OwnerShip,
                    LandNumber = a.LandNumber,
                    StreetWidth = a.StreetWidth,
                    NumberOfUnits = a.NumberOfUnits,
                    PlanNumber = a.PlanNumber,
                    MeterPrice = a.MeterPrice,
                    BuildingAge = a.BuildingAge,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    CityId = a.CityId,
                    CurrencyId = a.CurrencyId,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    CreateDate = a.CreatedDt,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    Rating = a.Rating
                }).ToList();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }

                if (user != null)
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.UserId == user.Id).AnyAsync();
                else
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.GuestId == guest.Id).AnyAsync();
            }

            return new PagingResponseForSuggesting<AnnouncementViewModel>
            {
                ItemCount = query.Count(),
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(query.Count(), model.Count))),
                DateFrom = null,
                Data = result,
                IsSaveFilter = f
            };
        }

        public async Task<PagingResponseModel<AnnouncementViewModel>> SimilarAnnouncementAsync(PagingRequestModel model,
            int announcementId, int userId, string deviceId, Language language)
        {
            IQueryable<Announcement> query;
            var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
                query = _repository.Filter<Announcement>(x => !x.IsDraft && x.Id != announcement.Id
                    && x.AnnouncementStatus == AnnouncementStatus.Accepted);
            }
            else
            {
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
                var myAnnouncementList = await _repository.Filter<Announcement>(a => a.UserId == user.Id).Select(a => a.Id).ToListAsync();
                var myFavouriteList = await _repository.Filter<Favourite>(a => a.UserId == user.Id).Select(a => a.AnnouncementId).ToListAsync();
                query = _repository.Filter<Announcement>(x => !x.IsDraft && x.Id != announcement.Id &&
                    x.AnnouncementStatus == AnnouncementStatus.Accepted && !myAnnouncementList.Contains(x.Id) && !myFavouriteList.Contains(x.Id));
            }

            var result = new AnnouncementLocateModel();
            query = query.Where(x => x.AnnouncementType == announcement.AnnouncementType);
            query = query.Where(x => x.AnnouncementEstateType == announcement.AnnouncementEstateType);
            if (announcement.AnnouncementResidentialType != null)
                query = query.Where(x => x.AnnouncementResidentialType == announcement.AnnouncementResidentialType);
            var announcements = query.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(a => a.CreatedDt)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    Price = Convert.ToInt64(a.Price),
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    Title = a.Title,
                    Description = a.Description,
                    UserId = a.UserId,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    TitleArabian = a.TitleArabian,
                    CountryId = a.CountryId,
                    CityId = a.CityId,
                    SittingCount = a.SittingCount,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    FacadeType = a.FacadeType,
                    CurrencySymbol = currency.Symbol,
                    CurrencyCode = currency.Code,
                    DisctrictName = a.DisctrictName,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    Lat = a.Lat,
                    Lng = a.Lng,
                    CreateDate = a.CreatedDt,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    MeterPrice = a.MeterPrice,
                    CurrencyId = a.CurrencyId,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    LandNumber = a.LandNumber,
                    PlanNumber = a.PlanNumber,
                    UserName = a.User.FullName,
                    StreetWidth = a.StreetWidth,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    CreatedDt = a.CreatedDt,
                    NumberOfVilla = a.NumberOfVilla,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    NumberOfShop = a.NumberOfShop,
                    NumberOfUnits = a.NumberOfUnits,
                    FireSystem = a.FireSystem,
                    LandCategory = a.LandCategory,
                    Rating = a.Rating,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                }).ToList();

            foreach (var ann in announcements)
            {
                City city = _repository.Filter<City>(c => c.Id == ann.CityId).Include(c => c.Country).FirstOrDefault();
                ann.City = city?.Name;
                ann.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    ann.Price *= currentRate;
                    ann.MeterPrice /= currentRate;
                }

                if (user != null)
                    ann.IsFavourite = await _repository
                         .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == ann.Id && x.UserId == user.Id).AnyAsync();
                else
                    ann.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == ann.Id && x.GuestId == guest.Id).AnyAsync();
            }
            int count = query.Count();
            return new PagingResponseModel<AnnouncementViewModel>
            {
                ItemCount = count,
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count))),
                DateFrom = null,
                Data = announcements
            };
        }

        public async Task<PagingResponseForSuggesting<AnnouncementViewModel>> NearbyMobileAsync(PagingRequestModel model,
            decimal lat, decimal lng, int userId, string deviceId, Language language)
        {
            Currency currency;
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception("User or guest not found"); //throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();

            var query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
              && x.AnnouncementStatus == AnnouncementStatus.Accepted).Include(x => x.Attachments);

            int count = await query.CountAsync();
            int page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));

            List<Announcement> announcements = new List<Announcement>();
            foreach (var a in query)
            {
                if (DistanceTo((double)a.Lat, (double)a.Lng, (double)lat, (double)lng) <= 20)
                {
                    announcements.Add(a);
                }
            }
            var result = announcements.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(a => a.CreatedDt).Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    Price = a.Price,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    DisctrictName = a.DisctrictName,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    PlanNumber = a.PlanNumber,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    CurrencyId = a.CurrencyId,
                    CurrencyCode = currency.Code,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    LandCategory = a.LandCategory,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    StreetWidth = a.StreetWidth,
                    LandNumber = a.LandNumber,
                    FurnishingStatus = a.FurnishingStatus,
                    Title = a.Title,
                    NumberOfUnits = a.NumberOfUnits,
                    FacadeType = a.FacadeType,
                    Description = a.Description,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    MeterPrice = a.MeterPrice,
                    CityId = a.CityId,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    CreateDate = a.CreatedDt,
                    Rating = a.Rating,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}"
                }).ToList();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }

                if (user != null)
                    r.IsFavourite = await _repository
                         .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.UserId == user.Id).AnyAsync();
                else
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.GuestId == guest.Id).AnyAsync();
            }

            return new PagingResponseForSuggesting<AnnouncementViewModel>
            {
                ItemCount = count,
                PageCount = page,
                DateFrom = null,
                Data = result,
                IsSaveFilter = false
            };
        }

        public async Task<PagingResponseForSuggesting<AnnouncementViewModel>> NearbyAsync(PagingRequestModel model,
            decimal lat, decimal lng, int userId, string deviceId, Language language)
        {
            Currency currency;
            Guest guest = new Guest();
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();

            var query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
              && x.AnnouncementStatus == AnnouncementStatus.Accepted).Include(x => x.Attachments);

            int count = await query.CountAsync();
            int page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));

            List<Announcement> announcements = new List<Announcement>();
            foreach (var a in query)
            {
                if (DistanceTo((double)a.Lat, (double)a.Lng, (double)lat, (double)lng) <= 20)
                {
                    announcements.Add(a);
                }
            }
            var result = announcements.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt).Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    Price = a.Price,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FacadeType = a.FacadeType,
                    CurrencyCode = currency.Code,
                    FurnishingStatus = a.FurnishingStatus,
                    Title = a.Title,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    LandCategory = a.LandCategory,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    CurrencySymbol = currency.Symbol,
                    DisctrictName = a.DisctrictName,
                    Description = a.Description,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    FireSystem = a.FireSystem,
                    OwnerShip = a.OwnerShip,
                    LandNumber = a.LandNumber,
                    StreetWidth = a.StreetWidth,
                    NumberOfUnits = a.NumberOfUnits,
                    PlanNumber = a.PlanNumber,
                    MeterPrice = a.MeterPrice,
                    BuildingAge = a.BuildingAge,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    CityId = a.CityId,
                    CurrencyId = a.CurrencyId,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, a.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, a.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, a.Id),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                            : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                            ConstValues.Width, ConstValues.Height, false, a.Id),
                    }).ToList(),
                    CreateDate = a.CreatedDt,
                    Rating = a.Rating,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}"
                }).ToList();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }

                if (user != null)
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.UserId == user.Id).AnyAsync();
                else
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.GuestId == guest.Id).AnyAsync();
            }

            return new PagingResponseForSuggesting<AnnouncementViewModel>
            {
                ItemCount = count,
                PageCount = page,
                DateFrom = null,
                Data = result,
                IsSaveFilter = false
            };
        }

        public async Task<PagingResponseAnnouncementFilter> AnnouncementFilterAsync
            (PagingRequestAnnouncementFilterModel model, int userId, string deviceId, Language language)
        {
            //deviceId = "42741dfb7e9cf2c3a1db8a4e7b5b0f80";
            int modelFilterCount = 0;
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();

            IQueryable<Announcement> query = null;
            switch (model.AnnouncementFilter.AnnouncementStatus)
            {
                case AnnouncementStatus.Featured:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked && x.TopAnnouncement);
                    break;
                case AnnouncementStatus.Rejected:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Rejected);
                    break;
                case AnnouncementStatus.Pending:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Pending);
                    break;
                case AnnouncementStatus.Expired:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Expired);
                    break;
                default:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Accepted);
                    break;
            }

            if (!string.IsNullOrEmpty(model.AnnouncementFilter.Search))
            {
                query = query.Where(a => a.Title.ToLower().Contains(model.AnnouncementFilter.Search.ToLower().Trim())
                    || a.TitleArabian.ToLower().Contains(model.AnnouncementFilter.Search.ToLower().Trim())
                    || a.Id.ToString().Contains(model.AnnouncementFilter.Search.Trim()));
                modelFilterCount += 1;
            }
            //Incorrect key AnnouncementiId
            if (model.AnnouncementFilter.AnnouncementiId > 0)
            {
                query = query.Where(x => x.Id == model.AnnouncementFilter.AnnouncementiId);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.BedroomCount != 0 && model.AnnouncementFilter.BedroomCount != null)
            {
                query = query.Where(x => x.BedroomCount == model.AnnouncementFilter.BedroomCount);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.BathroomCount != 0 && model.AnnouncementFilter.BathroomCount != null)
            {
                query = query.Where(x => x.BathroomCount == model.AnnouncementFilter.BathroomCount);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.PriceTo > 0 || model.AnnouncementFilter.PriceFrom > 0)
            {
                if (model.AnnouncementFilter.PriceFrom > 0 && model.AnnouncementFilter.PriceTo > 0)
                    query = query.Where(x => x.Price >= model.AnnouncementFilter.PriceFrom
                    && x.Price <= model.AnnouncementFilter.PriceTo);
                else if (model.AnnouncementFilter.PriceFrom > 0)
                    query = query.Where(x => x.Price >= model.AnnouncementFilter.PriceFrom);
                else if (model.AnnouncementFilter.PriceTo <= model.AnnouncementFilter.PriceTo)
                    query = query.Where(x => x.Price <= model.AnnouncementFilter.PriceTo);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.MinArea > 0 || model.AnnouncementFilter.MaxArea > 0)
            {
                if (model.AnnouncementFilter.MinArea > 0 && model.AnnouncementFilter.MaxArea > 0)
                    query = query.Where(x => x.Area >= model.AnnouncementFilter.MinArea && x.Area <= model.AnnouncementFilter.MaxArea);
                else if (model.AnnouncementFilter.MinArea > 0)
                    query = query.Where(x => x.Area >= model.AnnouncementFilter.MinArea);
                else if (model.AnnouncementFilter.MaxArea > 0)
                    query = query.Where(x => x.Area <= model.AnnouncementFilter.MaxArea);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.AnnouncementEstateType != null)
            {
                query = query.Where(x => x.AnnouncementEstateType == model.AnnouncementFilter.AnnouncementEstateType.Value);
                modelFilterCount += 1;
                if (model.AnnouncementFilter.AnnouncementEstateType.Value == AnnouncementEstateType.Land)
                {
                    if (model.AnnouncementFilter.FacadeType != null)
                        query = query.Where(x => x.FacadeType == model.AnnouncementFilter.FacadeType);
                    else if (model.AnnouncementFilter.LandType != null)
                        query = query.Where(x => x.LandType == model.AnnouncementFilter.LandType);
                    modelFilterCount += 1;
                }
            }
            if (model.AnnouncementFilter.AnnouncementType.HasValue)
            {
                query = query.Where(x => x.AnnouncementType == model.AnnouncementFilter.AnnouncementType);
                modelFilterCount += 1;
            }
            if (!string.IsNullOrEmpty(model.AnnouncementFilter.UserName))
            {
                query = query.Where(x => x.User.FullName.Contains(model.AnnouncementFilter.UserName));
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.ConstructionStatus.HasValue)
            {
                query = query.Where(x => x.ConstructionStatus == model.AnnouncementFilter.ConstructionStatus);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.FurnishingStatus.HasValue)
            {
                query = query.Where(x => x.FurnishingStatus == model.AnnouncementFilter.FurnishingStatus);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.OwnerShip.HasValue)
            {
                query = query.Where(x => x.OwnerShip == model.AnnouncementFilter.OwnerShip);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.AnnouncementResidentialType.HasValue)
            {
                query = query.Where(x => x.AnnouncementResidentialType == model.AnnouncementFilter.AnnouncementResidentialType);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.BuildingAge.HasValue)
            {
                query = query.Where(x => x.BuildingAge == model.AnnouncementFilter.BuildingAge);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.DateFrom != null || model.AnnouncementFilter.DateTo != null)
            {
                if (model.AnnouncementFilter.DateFrom > DateTime.MinValue && model.AnnouncementFilter.DateTo > DateTime.MinValue)
                    query = query.Where(x => x.CreatedDt.Date >= model.AnnouncementFilter.DateFrom.Value.Date
                     && x.CreatedDt.Date <= model.AnnouncementFilter.DateTo.Value.Date);
                else if (model.AnnouncementFilter.DateFrom > DateTime.MinValue)
                    query = query.Where(x => x.CreatedDt.Date >= model.AnnouncementFilter.DateFrom.Value.Date);
                else if (model.AnnouncementFilter.DateTo > DateTime.MinValue)
                    query = query.Where(x => x.CreatedDt.Date <= model.AnnouncementFilter.DateTo.Value.Date);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.Features.Count != 0)
            {
                //query = query.Where(x => model.AnnouncementFilter.Features.Contains
                //(x.Features.Select(f => f.FeatureType).ToList()));//.Contains(model.AnnouncementFilter.Features));
                foreach (var item in model.AnnouncementFilter.Features)
                {
                    query = query.Where(x => x.Features.Any(f => f.FeatureType == item));
                }
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.CountryId.HasValue)
            {
                query = query.Where(x => x.CountryId == model.AnnouncementFilter.CountryId);
                modelFilterCount += 1;
            }
            if (model.AnnouncementFilter.CityId.HasValue)
            {
                query = query.Where(x => x.CityId == model.AnnouncementFilter.CityId);
                modelFilterCount += 1;
            }
            var announcements = query.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(a => a.CreatedDt)
                .Select(a => new AnnouncementListViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                    Price = a.Price,
                    CreateDate = a.CreatedDt,
                    CreatedDt = a.CreatedDt,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    MeterPrice = a.MeterPrice,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    FacadeType = a.FacadeType,
                    CurrencyId = a.CurrencyId,
                    CurrencyCode = currency.Code,
                    CurrencySymbol = currency.Symbol,
                    CityId = a.CityId,
                    LandType = a.LandType,
                    LandCategory = a.LandCategory,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfUnits = a.NumberOfUnits,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    CommercialType = a.CommercialType,
                    FireSystem = a.FireSystem,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    DisctrictName = a.DisctrictName,
                    PlanNumber = a.PlanNumber,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                    Lat = a.Lat,
                    Lng = a.Lng,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    LandNumber = a.LandNumber,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    StreetWidth = a.StreetWidth,
                    NumberOfShop = a.NumberOfShop,
                    DateFrom = a.TopAnnouncementDayFrom,
                    DateTo = a.TopAnnouncementDayTo,
                    View = a.ViewsCount,
                    UserProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.ProfilePhoto, a.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, a.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.AnnouncementPhoto, h.File, false, a.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, a.Id),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                        : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                        ConstValues.Width, ConstValues.Height, false, a.Id),
                    }).ToList(),
                    Rating = a.Rating,
                }).ToList();

            foreach (var r in announcements)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price *= currentRate;
                    r.MeterPrice /= currentRate;
                }

                if (user != null)
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.UserId == user.Id).AnyAsync();
                else
                    r.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == r.Id && x.GuestId == guest.Id).AnyAsync();
            }

            if (model.AnnouncementFilter.SortingType.HasValue)
            {
                switch (model.AnnouncementFilter.SortingType)
                {
                    case SortingType.Featured:
                        break;
                    case SortingType.Newest:
                        announcements = announcements.OrderByDescending(a => a.CreatedDt).ToList();
                        break;
                    case SortingType.PriceLow:
                        announcements = announcements.OrderBy(a => a.Price).ToList();
                        break;
                    case SortingType.PriceHigh:
                        announcements = announcements.OrderByDescending(a => a.Price).ToList();
                        break;
                    case SortingType.BedsLeast:
                        announcements = announcements.OrderByDescending(a => a.BedroomCount).ToList();
                        break;
                    case SortingType.BedsMost:
                        announcements = announcements.OrderBy(a => a.BedroomCount).ToList();
                        break;
                    case SortingType.MostView:
                        announcements = announcements.OrderBy(a => a.View).ToList();
                        break;
                }
            }

            return new PagingResponseAnnouncementFilter
            {
                ModelFilterCount = modelFilterCount,
                DateFrom = model.Count == 1 ? query.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = query.Count(),
                Data = announcements.ToList(),
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(query.Count(), model.Count)))
            };
        }

        public int AnnouncementFilterPropertiesCount(FilterAnnouncementModel model)
        {
            int modelFilterCount = 0;

            if (model.AnnouncementStatus.HasValue)
                modelFilterCount += 1;
            if (model.SortingType.HasValue)
                modelFilterCount += 1;
            if (model.Lat != null && model.Lng != null)
                modelFilterCount += 1;
            if (!string.IsNullOrEmpty(model.Search))
                modelFilterCount += 1;
            if (model.SittingCount > 0)
                modelFilterCount += 1;
            if (model.SittingCount > 0)
                modelFilterCount += 1;
            if (model.BedroomCount != 0 && model.BedroomCount != null)
                modelFilterCount += 1;
            if (model.BathroomCount != 0 && model.BathroomCount != null)
                modelFilterCount += 1;
            if (model.PriceFrom.HasValue)
                modelFilterCount += 1;
            if (model.PriceTo.HasValue)
                modelFilterCount += 1;
            if (model.AnnouncementiId > 0)
                modelFilterCount += 1;
            if (model.MinArea.HasValue)
                modelFilterCount += 1;
            if (model.MaxArea.HasValue)
                modelFilterCount += 1;
            if (model.AnnouncementEstateType != null)
                modelFilterCount += 1;
            if (model.AnnouncementResidentialType != null)
                modelFilterCount += 1;
            if (model.AnnouncementType != null)
                modelFilterCount += 1;
            if (model.AnnouncementRentType != null)
                modelFilterCount += 1;
            if (!string.IsNullOrEmpty(model.UserName))
                modelFilterCount += 1;
            if (model.ConstructionStatus.HasValue)
                modelFilterCount += 1;
            if (model.SaleType.HasValue)
                modelFilterCount += 1;
            if (model.FurnishingStatus.HasValue)
                modelFilterCount += 1;
            if (model.OwnerShip.HasValue)
                modelFilterCount += 1;
            if (model.BuildingAge.HasValue)
                modelFilterCount += 1;
            if (model.DateFrom.HasValue)
                modelFilterCount += 1;
            if (model.DateTo.HasValue)
                modelFilterCount += 1;
            if (model.Features != null)
                modelFilterCount += 1;
            if (model.CountryId.HasValue)
                modelFilterCount += 1;
            if (model.CityId.HasValue)
                modelFilterCount += 1;
            if (model.LandCategory.HasValue)
                modelFilterCount += 1;

            return modelFilterCount;
        }

        public int AnnouncementFilterCount(FilterAnnouncementModel model,
            int userId, Language language, string deviceId)
        {
            IQueryable<Announcement> query = null;
            switch (model.AnnouncementStatus)
            {
                case AnnouncementStatus.Featured:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked && x.TopAnnouncement);
                    break;
                case AnnouncementStatus.Rejected:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Rejected);
                    break;
                case AnnouncementStatus.Pending:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Pending);
                    break;
                case AnnouncementStatus.Expired:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Expired);
                    break;
                default:
                    query = _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                        && x.AnnouncementStatus == AnnouncementStatus.Accepted);
                    break;
            }

            if (!string.IsNullOrEmpty(model.Search))
            {
                query = query.Where(a => a.Title.ToLower().Contains(model.Search.ToLower().Trim())
                    || a.TitleArabian.ToLower().Contains(model.Search.ToLower().Trim())
                    || a.Id.ToString().Contains(model.Search.Trim()));
            }
            //Incorrect key AnnouncementiId
            if (model.AnnouncementiId > 0)
                query = query.Where(x => x.Id == model.AnnouncementiId);
            if (model.BedroomCount != 0 && model.BedroomCount != null)
                query = query.Where(x => x.BedroomCount == model.BedroomCount);
            if (model.BathroomCount != 0 && model.BathroomCount != null)
                query = query.Where(x => x.BathroomCount == model.BathroomCount);
            if (model.PriceTo > 0 || model.PriceFrom > 0)
            {
                if (model.PriceFrom > 0 && model.PriceTo > 0)
                    query = query.Where(x => x.Price >= model.PriceFrom
                    && x.Price <= model.PriceTo);
                else if (model.PriceFrom > 0)
                    query = query.Where(x => x.Price >= model.PriceFrom);
                else if (model.PriceTo <= model.PriceTo)
                    query = query.Where(x => x.Price <= model.PriceTo);
            }
            if (model.MinArea > 0 || model.MaxArea > 0)
            {
                if (model.MinArea > 0 && model.MaxArea > 0)
                    query = query.Where(x => x.Area >= model.MinArea && x.Area <= model.MaxArea);
                else if (model.MinArea > 0)
                    query = query.Where(x => x.Area >= model.MinArea);
                else if (model.MaxArea > 0)
                    query = query.Where(x => x.Area <= model.MaxArea);
            }
            if (model.AnnouncementEstateType != null)
            {
                query = query.Where(x => x.AnnouncementEstateType == model.AnnouncementEstateType.Value);
                if (model.AnnouncementEstateType.Value == AnnouncementEstateType.Land)
                {
                    if (model.FacadeType != null)
                        query = query.Where(x => x.FacadeType == model.FacadeType);
                    else if (model.LandType != null)
                        query = query.Where(x => x.LandType == model.LandType);
                }
            }
            if (model.AnnouncementType.HasValue)
                query = query.Where(x => x.AnnouncementType == model.AnnouncementType);
            if (!string.IsNullOrEmpty(model.UserName))
                query = query.Where(x => x.User.FullName.Contains(model.UserName));
            if (model.ConstructionStatus.HasValue)
                query = query.Where(x => x.ConstructionStatus == model.ConstructionStatus);
            if (model.FurnishingStatus.HasValue)
                query = query.Where(x => x.FurnishingStatus == model.FurnishingStatus);
            if (model.OwnerShip.HasValue)
                query = query.Where(x => x.OwnerShip == model.OwnerShip);
            if (model.AnnouncementResidentialType.HasValue)
                query = query.Where(x => x.AnnouncementResidentialType == model.AnnouncementResidentialType);
            if (model.BuildingAge.HasValue)
                query = query.Where(x => x.BuildingAge == model.BuildingAge);
            if (model.DateFrom != null || model.DateTo != null)
            {
                if (model.DateFrom > DateTime.MinValue && model.DateTo > DateTime.MinValue)
                    query = query.Where(x => x.CreatedDt.Date >= model.DateFrom.Value.Date
                     && x.CreatedDt.Date <= model.DateTo.Value.Date);
                else if (model.DateFrom > DateTime.MinValue)
                    query = query.Where(x => x.CreatedDt.Date >= model.DateFrom.Value.Date);
                else if (model.DateTo > DateTime.MinValue)
                    query = query.Where(x => x.CreatedDt.Date <= model.DateTo.Value.Date);
            }
            if (model.Features.Count != 0)
            {
                foreach (var item in model.Features)
                {
                    query = query.Where(x => x.Features.Any(f => f.FeatureType == item));
                }
            }
            if (model.CountryId.HasValue)
                query = query.Where(x => x.CountryId == model.CountryId);
            if (model.CityId.HasValue)
                query = query.Where(x => x.CityId == model.CityId);
            if (!string.IsNullOrEmpty(model.Search))
            {
                var search = model.Search.ToLower();
                var searchParams = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in searchParams)
                {
                    query = query.Where(x => x.Title.ToLower().Contains(item));
                }
            }
            return query.Count();
        }

        public async Task<IEnumerable<AnnouncementViewModel>> MapSmallRadius(MapFilterAnnouncementModel model, int userId, Language language, string deviceId)
        {
            Currency currency;
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();

            var keyValue = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("lat", model.Lat),
                new KeyValuePair<string, object>("lng", model.Lng),
                new KeyValuePair<string, object>("radius", model.Distance)
            };

            var announcementDistances = _repository.Execute<NearbyProcedureResponse>("GetWithinCustomRadius", keyValue.ToArray()).ToList();
            var announcementIds = announcementDistances.Select(x => x.Id).ToList();
            var query = _repository.Filter<Announcement>(x => !x.IsDraft && announcementIds.Contains(x.Id) && x.AnnouncementStatus == AnnouncementStatus.Accepted);
            if (model.BedroomCount != null && model.BedroomCount != 0)
                query = query.Where(x => x.BedroomCount == model.BedroomCount);
            if (model.BathroomCount != null && model.BathroomCount != 0)
                query = query.Where(x => x.BathroomCount == model.BathroomCount);
            if (model.PriceFrom != null && model.PriceFrom != 0)
                query = query.Where(x => x.Price >= model.PriceFrom);
            if (model.PriceTo != null && model.PriceTo != 0)
                query = query.Where(x => x.Price <= model.PriceTo);
            if (model.MinArea != null && model.MinArea != 0)
                query = query.Where(x => x.Area >= model.MinArea);
            if (model.MaxArea != null && model.MaxArea != 0)
                query = query.Where(x => x.Area <= model.MaxArea);
            if (model.AnnouncementEstateType != null)
                query = query.Where(x => x.AnnouncementEstateType == model.AnnouncementEstateType);
            if (model.CountryId.HasValue)
                query = query.Where(x => x.CountryId == model.CountryId);
            if (model.CityId.HasValue)
                query = query.Where(x => x.CityId == model.CityId);
            if (model.SittingCount > 0)
                query = query.Where(a => a.SittingCount == model.SittingCount);
            if (!string.IsNullOrEmpty(model.UserName))
                query = query.Where(x => x.User.FullName.Contains(model.UserName));
            if (model.FurnishingStatus.HasValue)
                query = query.Where(x => x.FurnishingStatus == model.FurnishingStatus);
            if (model.OwnerShip.HasValue)
                query = query.Where(x => x.OwnerShip == model.OwnerShip);
            if (model.BuildingAge.HasValue)
                query = query.Where(x => x.BuildingAge == model.BuildingAge);
            if (model.AnnouncementEstateType.HasValue)
            {
                if (model.AnnouncementEstateType.Value == AnnouncementEstateType.Commercial)
                    query = query.Where(x => x.CommercialType == model.CommercialType);
                if (model.AnnouncementEstateType.Value == AnnouncementEstateType.Land)
                    query = query.Where(x => x.LandType == model.LandType);
            }
            if (model.FacadeType != null)
                query = query.Where(x => x.FacadeType == model.FacadeType);
            if (model.MinMeterPrice.HasValue)
                query = query.Where(x => x.MeterPrice >= model.MinMeterPrice);
            if (model.MaxMeterPrice.HasValue)
                query = query.Where(x => x.MeterPrice <= model.MaxMeterPrice);
            if (!string.IsNullOrEmpty(model.DisctrictName))
                query = query.Where(x => x.DisctrictName == model.DisctrictName);
            if (model.DateFrom != null || model.DateTo != null)
            {
                if (model.DateFrom != null && model.DateTo != null)
                    query = query.Where(x => x.CreatedDt.Date >= model.DateFrom.Value.Date
                     && x.CreatedDt.Date <= model.DateTo.Value.Date);
                else if (model.DateFrom != null)
                    query = query.Where(x => x.CreatedDt.Date >= model.DateFrom.Value.Date);
                else if (model.DateTo != null)
                    query = query.Where(x => x.CreatedDt.Date <= model.DateTo.Value.Date);
            }
            if (model.AnnouncementType != null)
            {
                query = query.Where(x => x.AnnouncementType == model.AnnouncementType);
                if (model.AnnouncementType == AnnouncementType.Rent)
                    query = query.Where(x => x.AnnouncementRentType == model.AnnouncementRentType);
            }
            if (model.Features != null)
                foreach (var item in model.Features)
                {
                    query = query.Where(x => x.Features.Any(f => f.FeatureType == item));
                }
            var result = query.Select(a => new AnnouncementViewModel
            {
                Id = a.Id,
                AnnouncementType = a.AnnouncementType,
                AnnouncementEstateType = a.AnnouncementEstateType,
                AnnouncementRentType = a.AnnouncementRentType,
                AnnouncementResidentialType = a.AnnouncementResidentialType,
                AnnouncementStatus = a.AnnouncementStatus,
                Price = Convert.ToInt64(a.Price),
                Area = Convert.ToInt64(a.Area),
                BathroomCount = a.BathroomCount,
                BedroomCount = a.BedroomCount,
                Title = a.Title,
                Description = a.Description,
                TitleArabian = a.TitleArabian,
                Address = language == Language.English ? a.AddressEn.Trim() : a.AddressAr != null ? a.AddressAr.Trim() : null,
                CountryId = a.CountryId,
                CityId = a.CityId,
                SittingCount = a.SittingCount,
                ConstructionStatus = a.ConstructionStatus,
                SaleType = a.SaleType,
                FurnishingStatus = a.FurnishingStatus,
                OwnerShip = a.OwnerShip,
                BuildingAge = a.BuildingAge,
                CommercialType = a.CommercialType,
                LandType = a.LandType,
                FacadeType = a.FacadeType,
                DisctrictName = a.DisctrictName,
                ShareUrl = $"https://baitkm.com/products/details/{a.Id}",
                Lat = a.Lat,
                Lng = a.Lng,
                CreateDate = a.CreatedDt,
                BalconyArea = a.BalconyArea,
                KitchenArea = a.KitchenArea,
                MeterPrice = a.MeterPrice,
                LaundryArea = a.LaundryArea,
                LivingArea = a.LivingArea,
                LandNumber = a.LandNumber,
                UserId = a.UserId,
                PlanNumber = a.PlanNumber,
                UserName = a.User.FullName,
                CurrencySymbol = currency.Symbol,
                CurrencyCode = currency.Code,
                StreetWidth = a.StreetWidth,
                NumberOfAppartment = a.NumberOfAppartment,
                NumberOfFloors = a.NumberOfFloors,
                CreatedDt = a.CreatedDt,
                NumberOfVilla = a.NumberOfVilla,
                OfficeSpace = a.OfficeSpace,
                LaborResidence = a.LaborResidence,
                District = a.District,
                NumberOfWareHouse = a.NumberOfWareHouse,
                NumberOfShop = a.NumberOfShop,
                NumberOfUnits = a.NumberOfUnits,
                FireSystem = a.FireSystem,
                LandCategory = a.LandCategory,
                CurrencyId = a.CurrencyId,
                Rating = a.Rating,
                Photo = new ImageOptimizer
                {
                    Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                    PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                }
            }).ToList();

            foreach (var variable in result)
            {
                City city = _repository.Filter<City>(c => c.Id == variable.CityId).Include(c => c.Country).FirstOrDefault();
                variable.City = city?.Name;
                variable.Country = city?.Country.Name;
                if (currency.Id != 1 && variable.CurrencyId != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == variable.CurrencyId).FirstOrDefault().CurrentRate;
                    variable.Price *= currentRate;
                    variable.MeterPrice /= currentRate;
                }

                if (user != null)
                    variable.IsFavourite = await _repository
                         .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == variable.Id && x.UserId == user.Id).AnyAsync();
                else
                    variable.IsFavourite = await _repository
                        .FilterAsNoTracking<Favourite>(x => x.AnnouncementId == variable.Id && x.GuestId == guest.Id).AnyAsync();
            }
            return result;
        }

        public async Task<DashboardViewModel> DashboardPendingListAdmin(int userId)
        {
            var caller = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var activeAnnouncementCount = await _repository
                .FilterAsNoTracking<Announcement>(x => !x.IsDraft && x.AnnouncementStatus == AnnouncementStatus.Accepted).CountAsync();
            var featuredAnnouncementCount = await
                _repository.FilterAsNoTracking<Announcement>(x => !x.IsDraft && x.TopAnnouncement).CountAsync();
            var androidCount = await _repository.Filter<User>(x => x.OsType == OsType.Android).CountAsync();
            var iosCount = await _repository.Filter<User>(x => x.OsType == OsType.Ios).CountAsync();
            var webCount = await _repository.Filter<User>(x => x.OsType == OsType.Web).CountAsync();
            var unreadSupportMessagesCount = await _repository.Filter<SupportMessage>(x => !x.IsSeen
                && x.UserSenderId != caller.Id).Select(y => y.SupportConversationId).Distinct().CountAsync();
            return new DashboardViewModel
            {
                ActiveAnnouncementCount = activeAnnouncementCount,
                FeaturedAnnouncementCount = featuredAnnouncementCount,
                UnreadSupportMessagesCount = unreadSupportMessagesCount,
                AndroidCount = androidCount,
                IosCount = iosCount,
                WebCount = webCount
            };
        }

        public List<StatisticViewModel> DashboardStatistic()
        {
            var keyValue = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("days_count", 7)
            };
            var stats = _repository.Execute<StatisticViewModel>("StatisticsProcedure", keyValue.ToArray()).ToList();
            List<StatisticViewModel> statistics = stats;
            return statistics;
        }

        public GetDashboardAnnouncementStatisticResponseModel DashboardAnnouncementStatistic
            (int userId, GetDashboardAnnouncementStatisticRequestModel model)
        {
            var caller = _repository.Filter<User>(u => u.Id == userId && u.RoleEnum == Role.Admin).FirstOrDefault();
            if (caller == null)
                throw new Exception(_optionsBinder.Error().DeleteUsers);

            var announcements = _repository.FilterAsNoTracking<Announcement>(a => !a.IsDraft
                && a.AnnouncementStatus == AnnouncementStatus.Accepted && !a.User.IsBlocked);
            if (model.StartDate != null)
                announcements = announcements.Where(a => a.CreatedDt.Date >= model.StartDate.Value.Date);
            if (model.EndDate != null)
                announcements = announcements.Where(a => a.CreatedDt.Date <= model.EndDate.Value.Date);

            var test = _repository.FilterAsNoTracking<Announcement>(a => !a.IsDraft
                && a.AnnouncementStatus == AnnouncementStatus.Accepted && !a.User.IsBlocked
                && a.CreatedDt.Date >= model.StartDate.Value.Date && a.CreatedDt.Date <= model.EndDate.Value.Date);

            return new GetDashboardAnnouncementStatisticResponseModel
            {
                SaleCount = announcements.Count(a => a.AnnouncementType == AnnouncementType.Sale),
                RentCount = announcements.Count(a => a.AnnouncementType == AnnouncementType.Rent),
                ResidentalCount = announcements.Count(a => a.AnnouncementEstateType == AnnouncementEstateType.Residential),
                ComercialCount = announcements.Count(a => a.AnnouncementEstateType == AnnouncementEstateType.Commercial),
                LandCount = announcements.Count(a => a.AnnouncementEstateType == AnnouncementEstateType.Land),
            };
        }

        public async Task<PagingResponseModel<AnnouncementViewModel>> GetAnnouncementListAsync(PagingRequestModel model, int userId)
        {
            var caller = _repository.Filter<User>(u => u.Id == userId).FirstOrDefault();
            var currency = await _repository.Filter<Currency>(c => c.Id == caller.CurrencyId).FirstOrDefaultAsync();
            var announcements = _repository.FilterAsNoTracking<Announcement>(x => !x.IsDraft
                 && x.AnnouncementStatus == AnnouncementStatus.Accepted && !x.User.IsBlocked);
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(announcements.Count(), model.Count)));
            var result = announcements.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(a => a.CreatedDt).Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    AnnouncementEstateType = a.AnnouncementEstateType,
                    AnnouncementType = a.AnnouncementType,
                    AnnouncementStatus = a.AnnouncementStatus,
                    AnnouncementRentType = a.AnnouncementRentType,
                    AnnouncementResidentialType = a.AnnouncementResidentialType,
                    Area = Convert.ToInt64(a.Area),
                    BathroomCount = a.BathroomCount,
                    BedroomCount = a.BedroomCount,
                    Address = a.AddressEn.Trim(),
                    Price = a.Price,
                    Title = a.Title,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    CreateDate = a.CreatedDt,
                    BalconyArea = a.BalconyArea,
                    KitchenArea = a.KitchenArea,
                    LaundryArea = a.LaundryArea,
                    LivingArea = a.LivingArea,
                    SittingCount = a.SittingCount,
                    Floor = a.Floor,
                    ConstructionStatus = a.ConstructionStatus,
                    SaleType = a.SaleType,
                    FurnishingStatus = a.FurnishingStatus,
                    OwnerShip = a.OwnerShip,
                    BuildingAge = a.BuildingAge,
                    CurrencyId = a.CurrencyId,
                    CityId = a.CityId,
                    LandNumber = a.LandNumber,
                    PlanNumber = a.PlanNumber,
                    CurrencyCode = currency.Code,
                    CurrencySymbol = currency.Symbol,
                    NumberOfUnits = a.NumberOfUnits,
                    CommercialType = a.CommercialType,
                    LandType = a.LandType,
                    DisctrictName = a.DisctrictName,
                    StreetWidth = a.StreetWidth,
                    MeterPrice = a.MeterPrice,
                    FacadeType = a.FacadeType,
                    NumberOfAppartment = a.NumberOfAppartment,
                    NumberOfFloors = a.NumberOfFloors,
                    NumberOfVilla = a.NumberOfVilla,
                    NumberOfShop = a.NumberOfShop,
                    LandCategory = a.LandCategory,
                    FireSystem = a.FireSystem,
                    OfficeSpace = a.OfficeSpace,
                    LaborResidence = a.LaborResidence,
                    District = a.District,
                    NumberOfWareHouse = a.NumberOfWareHouse,
                    Rating = a.Rating,
                    Features = a.Features.Select(s => s.FeatureType).ToList(),
                    UserProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, a.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.AnnouncementBasePhoto, a.BasePhoto, 100, 100, true, 0)
                    },
                    Photos = a.Attachments.Where(y => y.AttachmentType == AttachmentType.OtherImages).Select(h => new ImageAndVideoOptimizer
                    {
                        Photo = Path.GetExtension(h.File).ToLower() == ".mp4" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                           UploadType.AnnouncementPhoto, h.File, false, a.Id) : Path.GetExtension(h.File).ToLower() == ".mov" ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                           UploadType.AnnouncementPhoto, h.File, false, a.Id) : Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                           UploadType.AnnouncementPhoto, h.File, ConstValues.Width, ConstValues.Height, false, a.Id),
                        ThumbNail = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                           UploadType.AnnouncementPhoto, Path.GetExtension(h.File).ToLower() == ".mp4" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg"
                           : Path.GetExtension(h.File).ToLower() == ".mov" ? $"{Path.GetFileNameWithoutExtension(h.File)}_thumb.jpg" : null,
                           ConstValues.Width, ConstValues.Height, false, a.Id),
                    }).ToList()
                }).AsEnumerable();

            foreach (var r in result)
            {
                City city = _repository.Filter<City>(c => c.Id == r.CityId).Include(c => c.Country).FirstOrDefault();
                r.City = city?.Name;
                r.Country = city?.Country.Name;
                if (currency.Id != 1)
                {
                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                    r.Price /= currentRate;
                    r.MeterPrice /= currentRate;
                }
            }
            return new PagingResponseModel<AnnouncementViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? announcements.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = announcements.Count(),
                PageCount = page
            };
        }

        public async Task<bool> AddToTopListAsync(AddToTopListModel model, Language language, int announcementId)
        {
            var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && !x.User.IsBlocked
                && x.Id == announcementId).FirstOrDefaultAsync();
            if (model.Day == 0)
                throw new Exception(_optionsBinder.Error().FillDay);
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            announcement.TopAnnouncement = true;
            announcement.TopAnnouncementDayFrom = DateTime.UtcNow;
            announcement.TopAnnouncementDayTo = DateTime.UtcNow.AddDays(model.Day);
            _repository.Update(announcement);
            await _repository.SaveChangesAsync();
            if (!announcement.TopAnnouncement)
                return true;
            var date = new DateTime(announcement.AnnouncementStatusLastDay.Year, announcement.AnnouncementStatusLastDay.Month,
                announcement.AnnouncementStatusLastDay.Day, announcement.AnnouncementStatusLastDay.Hour,
                announcement.AnnouncementStatusLastDay.Minute, announcement.AnnouncementStatusLastDay.Second);
            var deactivationTopDayTriggerBuilder = TriggerBuilder.Create()
                .WithIdentity($"{nameof(AnnouncementTopDeactivationDayJob)}Trigger#{AnnouncementTopDeactivationDayJob.Name}{announcement.Id}")
                .StartAt(date);
            dynamic deactivationTopDay = new ExpandoObject();
            deactivationTopDay.announcementId = announcement.Id;
            await SchedulerHelper.Schedule<AnnouncementTopDeactivationDayJob, IJobListener>(new QuartzScheduleModel
            {
                Name = $"{AnnouncementTopDeactivationDayJob.Name}{announcement.Id}",
                IsListenerRequested = false,
                DataMap = deactivationTopDay,
                TriggerBuilder = deactivationTopDayTriggerBuilder
            });
            return true;
        }

        //TO DO - test
        //public async Task<bool> Delete(int announcementId, string userName, VerifiedBy verifiedBy)
        //{
        //    var user = await _repository.Filter<User>(x => !x.IsBlocked
        //        && (x.Email == userName || (x.PhoneCode + x.Phone) == userName) && x.VerifiedBy == verifiedBy).FirstOrDefaultAsync();
        //    var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft
        //        && x.Id == announcementId && x.UserId == user.Id).FirstOrDefaultAsync();
        //    announcement.CheckIfExist();
        //    var announcementPhotos = await _repository.Filter<AnnouncementAttachment>(x => x.AnnouncementId == announcement.Id)
        //        .Select(x => x.Id).ToListAsync();
        //    var favoruites = await _repository.Filter<Favourite>(x => x.AnnouncementId == announcement.Id)
        //        .Select(x => x.Id).ToListAsync();
        //    var guestFavourite = await _repository.Filter<GuestFavourite>(x => x.AnnouncementId == announcement.Id)
        //        .Select(x => x.Id).ToListAsync();
        //    var announcementFeatures = await _repository.Filter<AnnouncementFeature>(x => x.AnnouncementId == announcement.Id)
        //       .Select(x => x.Id).ToListAsync();
        //    var announcementReport = await _repository.Filter<AnnouncementReport>(x => x.AnnouncementId == announcement.Id)
        //       .Select(x => x.Id).ToListAsync();
        //    var notifications = await _repository.Filter<PersonNotification>(x => x.AnnouncementId == announcement.Id)
        //       .Select(x => x.Id).ToListAsync();
        //    await _repository.RemoveRange<AnnouncementAttachment>(announcementPhotos);
        //    await _repository.RemoveRange<Favourite>(favoruites);
        //    await _repository.RemoveRange<GuestFavourite>(guestFavourite);
        //    await _repository.RemoveRange<AnnouncementFeature>(announcementFeatures);
        //    await _repository.RemoveRange<AnnouncementReport>(announcementReport);
        //    await _repository.RemoveRange<PersonNotification>(notifications);
        //    var supportMessage = await _repository.Filter<SupportMessage>(x => x.SupportMessageBodyType ==
        //    SupportMessageBodyType.Announcement && x.MessageText.Contains(announcement.Id.ToString())).Select(x => x.Id).ToListAsync();
        //    await _repository.RemoveRange<SupportMessage>(supportMessage);
        //    List<int> message = new List<int>();
        //    await _repository.Remove<Announcement>(announcement.Id);
        //    var name = $"{AnnouncementDeactivationDayJob.Name}{announcement.Id}";
        //    await SchedulerHelper.UnSchedule<AnnouncementDeactivationDayJob>(name);
        //    await _repository.SaveChangesAsync();
        //    return true;
        //}

        public async Task<PagingResponseModel<AnnouncementReportModel>> AnnouncementReportListAsync
            (PagingRequestModel model, string userCurrency, Language language)
        {
            var announcementReport = _repository.GetAll<AnnouncementReport>();
            var result = await announcementReport.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.CreatedDt)
                .Select(ar => new AnnouncementReportModel
                {
                    Id = ar.Id,
                    AnnouncementId = ar.AnnouncementId,
                    CreateDate = ar.CreatedDt,
                    Price = Math.Round(ar.Announcement.Price, 2),
                    Address = language == Language.English ? ar.Announcement.AddressEn.Trim() : ar.Announcement.AddressAr != null ? ar.Announcement.AddressAr.Trim() : null,
                    Title = ar.Announcement.Title,
                    Description = ar.Description,
                    AnnouncementStatus = ar.Announcement.AnnouncementStatus,
                    AnnouncementRentType = ar.Announcement.AnnouncementRentType,
                    ReportStatus = ar.ReportAnnouncementStatus,
                    UserId = ar.UserId,
                    UserName = ar.User.FullName,
                    UserProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, ar.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, ar.Announcement.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, ar.Announcement.BasePhoto, 100, 100, true, 0)
                    }
                }).ToListAsync();
            return new PagingResponseModel<AnnouncementReportModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? announcementReport.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = announcementReport.Count(),
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(announcementReport.Count(), model.Count)))
            };
        }

        public async Task<PagingResponseReportFilter> ReportFilter(PagingRequestReportFilterModel model, Language language)
        {
            IQueryable<AnnouncementReport> query;
            query = _repository.Filter<AnnouncementReport>(x => !x.User.IsBlocked);
            if (model.ReportAnnouncementStatus != null)
                query = _repository.Filter<AnnouncementReport>(x => x.ReportAnnouncementStatus == model.ReportAnnouncementStatus);
            var result = new AnnouncementLocateModel();
            if (!string.IsNullOrEmpty(model.AnnouncementFilter.Address))
            {
                var addressUri = new Uri($"https://maps.googleapis.com/maps/api/geocode/json?address={model.AnnouncementFilter.Address}{ConstValues.GoogleLocateKey}");
                using (var client = new HttpClient())
                {
                    var resp = await client.GetAsync(addressUri);
                    var json = await resp.Content.ReadAsStringAsync();
                    var token = JToken.Parse(json);
                    var array = token.SelectToken("results");
                    var deserialized = JsonConvert.DeserializeObject<List<UserLocationParsingBase>>(array.ToString());
                    var item = deserialized.FirstOrDefault();
                    result.Lat = item?.Geometry.Location.Lat ?? 0.0m;
                    result.Lng = item?.Geometry.Location.Lng ?? 0.0m;
                }
            }
            if (model.AnnouncementFilter.BedroomCount != 0 && model.AnnouncementFilter.BedroomCount != null)
                query = query.Where(x => x.Announcement.BedroomCount == model.AnnouncementFilter.BedroomCount);
            if (model.AnnouncementFilter.BathroomCount != 0 && model.AnnouncementFilter.BathroomCount != null)
                query = query.Where(x => x.Announcement.BathroomCount == model.AnnouncementFilter.BathroomCount);
            if (model.AnnouncementFilter.PriceTo > 0 && model.AnnouncementFilter.PriceFrom > 0)
            {
                if (model.AnnouncementFilter.PriceFrom > 0 && model.AnnouncementFilter.PriceTo > 0)
                    query = query.Where(x => x.Announcement.Price >= model.AnnouncementFilter.PriceFrom
                    && x.Announcement.Price <= model.AnnouncementFilter.PriceTo);
                else if (model.AnnouncementFilter.PriceFrom > 0)
                    query = query.Where(x => x.Announcement.Price >= model.AnnouncementFilter.PriceFrom);
                else if (model.AnnouncementFilter.PriceTo <= model.AnnouncementFilter.PriceTo)
                    query = query.Where(x => x.Announcement.Price <= model.AnnouncementFilter.PriceTo);
            }
            if (model.AnnouncementFilter.MinArea > 0 || model.AnnouncementFilter.MaxArea > 0)
            {
                if (model.AnnouncementFilter.MinArea > 0 && model.AnnouncementFilter.MaxArea > 0)
                    query = query.Where(x => x.Announcement.Area >= model.AnnouncementFilter.MinArea && x.Announcement.Area <= model.AnnouncementFilter.MaxArea);
                else if (model.AnnouncementFilter.MinArea > 0)
                    query = query.Where(x => x.Announcement.Area >= model.AnnouncementFilter.MinArea);
                else if (model.AnnouncementFilter.MaxArea > 0)
                    query = query.Where(x => x.Announcement.Area <= model.AnnouncementFilter.MaxArea);
            }
            if (!string.IsNullOrEmpty(model.AnnouncementFilter.Address))
            {
                var lat = result.Lat;
                var lng = result.Lng;
                if (lat < 0)
                    lat *= -1;
                if (lng < 0)
                    lng *= -1;
                query = query.Where(x =>
                    Math.Abs(Math.Abs(x.Announcement.Lat) - lat) < 0.01m && Math.Abs(Math.Abs(x.Announcement.Lng) - lng) < 0.01m);
            }
            if (model.AnnouncementFilter.AnnouncementEstateType != null)
                query = query.Where(x => x.Announcement.AnnouncementEstateType == model.AnnouncementFilter.AnnouncementEstateType);
            if (model.AnnouncementFilter.AnnouncementResidentialType != null)
                query = query.Where(x => x.Announcement.AnnouncementResidentialType == model.AnnouncementFilter.AnnouncementResidentialType);
            if (model.AnnouncementFilter.AnnouncementType != null)
                query = query.Where(x => x.Announcement.AnnouncementType == model.AnnouncementFilter.AnnouncementType);
            if (model.AnnouncementFilter.AnnouncementRentType != null)
                query = query.Where(x => x.Announcement.AnnouncementRentType == model.AnnouncementFilter.AnnouncementRentType);
            if (!string.IsNullOrEmpty(model.AnnouncementFilter.UserName))
                query = query.Where(x => x.User.FullName.Contains(model.AnnouncementFilter.UserName));
            if (model.AnnouncementFilter.DateFrom != null || model.AnnouncementFilter.DateTo != null)
            {
                if (model.AnnouncementFilter.DateFrom != null && model.AnnouncementFilter.DateTo != null)
                    query = query.Where(x => x.CreatedDt >= model.AnnouncementFilter.DateFrom
                    && x.CreatedDt <= model.AnnouncementFilter.DateTo);
                else if (model.AnnouncementFilter.DateFrom != null)
                    query = query.Where(x => x.CreatedDt >= model.AnnouncementFilter.DateFrom);
                else if (model.AnnouncementFilter.DateTo != null)
                    query = query.Where(x => x.CreatedDt <= model.AnnouncementFilter.DateTo);
            }
            if (model.AnnouncementFilter.Features.Count != 0)
            {
                foreach (var item in model.AnnouncementFilter.Features)
                {
                    query = query.Where(x => x.Announcement.Features.Any(f => f.FeatureType == item));
                }
            }
            if (!string.IsNullOrEmpty(model.AnnouncementFilter.Search))
                query = query.Where(x => x.Announcement.Title.Contains(model.AnnouncementFilter.Search));
            var announcements = query.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(ar => ar.CreatedDt).Select(ar => new AnnouncementReportModel
                {
                    Id = ar.Id,
                    AnnouncementId = ar.AnnouncementId,
                    CreateDate = ar.CreatedDt,
                    Price = Math.Round(ar.Announcement.Price, 2),
                    Address = language == Language.English ? ar.Announcement.AddressEn.Trim() : ar.Announcement.AddressAr != null ? ar.Announcement.AddressAr.Trim() : null,
                    Title = ar.Announcement.Title,
                    Description = ar.Description,
                    AnnouncementStatus = ar.Announcement.AnnouncementStatus,
                    AnnouncementRentType = ar.Announcement.AnnouncementRentType,
                    ReportStatus = ar.ReportAnnouncementStatus,
                    UserId = ar.UserId,
                    UserName = ar.User.FullName,
                    UserProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.ProfilePhoto, ar.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, ar.Announcement.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                }).AsEnumerable();
            //var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(announcements.Count(), model.Count)));
            return new PagingResponseReportFilter
            {
                AnnouncementResponseFilter = announcements,
                DateFrom = model.Count == 1 ? query.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = announcements.Count(),
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(announcements.Count(), model.Count)))
            };
        }

        public async Task<bool> AddReportAsync(AnnouncementReportAddModel model, int userId)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var announcement = await _repository.FilterAsNoTracking<Announcement>(a => a.Id == model.AnnouncementId && !a.IsDraft).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);

            var announcementReport = new AnnouncementReport
            {
                UserId = user.Id,
                AnnouncementId = announcement.Id,
                Description = model.Description,
            };
            _repository.Create(announcementReport);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveReportAsync(int announcementId)
        {
            var announcement = await _repository.Filter<Announcement>(a => !a.IsDraft && a.Id == announcementId)
                .Include(a => a.AnnouncementReports).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            if (announcement.AnnouncementReports.Count() == 0)
                throw new Exception("Report about this announcement not found");
            announcement.AnnouncementStatus = AnnouncementStatus.Rejected;
            _repository.Update(announcement);
            foreach (var item in announcement.AnnouncementReports)
            {
                item.ReportAnnouncementStatus = AnnouncementStatus.Accepted;
            }
            _repository.UpdateRange(announcement.AnnouncementReports);
            await _repository.SaveChangesAsync();
            return true;
        }

        //TO DO - Check logic
        public async Task<bool> RejectReportAsync(int announcementId)
        {
            var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            var announcementReport = await _repository.Filter<AnnouncementReport>(x => x.AnnouncementId == announcementId).FirstOrDefaultAsync();
            if (announcementReport != null)
                throw new Exception("Announcement Report not found");
            announcement.AnnouncementStatus = AnnouncementStatus.Accepted;
            _repository.Update(announcement);
            announcementReport.ReportAnnouncementStatus = AnnouncementStatus.Rejected;
            _repository.Update(announcementReport);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HideAnnouncementAsync(int announcementId, int userId)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var announcement = await _repository.Filter<Announcement>(a => !a.IsDraft && a.Id == announcementId).FirstOrDefaultAsync();
            if (announcement.UserId != user.Id)
                throw new Exception("You have not participant to hide this announcement");
            dynamic property = new ExpandoObject();
            announcement.AnnouncementStatus = AnnouncementStatus.Hidden;
            announcement.AnnouncementChangedDay = DateTime.Today;
            var name = $"{AnnouncementDeactivationDayJob.Name}{announcement.Id}";
            await SchedulerHelper.UnSchedule<AnnouncementDeactivationDayJob>(name);
            _repository.Update(announcement);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MakeActiveAnnouncementAsync(int announcementId, int userId)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var announcement = await _repository.Filter<Announcement>(a => !a.IsDraft && a.Id == announcementId
                && a.AnnouncementStatus == AnnouncementStatus.Hidden).FirstOrDefaultAsync();
            if (announcement.UserId != user.Id)
                throw new Exception("You have not participant to hide this announcement");
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            announcement.AnnouncementStatus = AnnouncementStatus.Accepted;
            _repository.Update(announcement);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAnnouncement(AnnouncementRejectModel model, Language language, int announcementId)
        {
            Notification reject = null;
            var announcement = await _repository.FilterAsNoTracking<Announcement>(a => a.Id == announcementId
                && !a.IsDraft && !a.User.IsBlocked).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            var userLanguage = await _repository.FilterAsNoTracking<PersonOtherSetting>(x =>
                x.UserId == announcement.UserId).ToListAsync();
            if (model.NotificationType == AnnouncementNotificationType.Reject)
                reject = await _repository.Filter<Notification>(n => n.Id == model.Id).Include(s => s.NotificationTranslate).FirstOrDefaultAsync();
            else
            {
                if (model.NotificationType == AnnouncementNotificationType.OtherReason &&
                (model.DescriptionEnglish == null || model.DescriptionArabian == null))
                    throw new Exception(_optionsBinder.Error().Reason);
                reject = _repository.Create(new Notification
                {
                    Title = "Your announcement has been rejected",
                    Description = model.DescriptionEnglish,
                    NotificationType = AnnouncementNotificationType.OtherReason
                });
                _repository.Create(new NotificationTranslate
                {
                    NotificationId = reject.Id,
                    Title = "تم رفض إعلانك",
                    Description = model.DescriptionArabian,
                    Language = Language.Arabian
                });
            }
            _repository.Create(new PersonNotification
            {
                AnnouncementId = announcement.Id,
                UserId = announcement.UserId,
                NotificationId = reject.Id
            });
            announcement.AnnouncementStatus = AnnouncementStatus.Rejected;
            _repository.Update(announcement);
            var name = $"{AnnouncementDeactivationDayJob.Name}{announcement.Id}";
            await SchedulerHelper.UnSchedule<AnnouncementDeactivationDayJob>(name);
            await _repository.SaveChangesAsync();
            var lang = userLanguage.Select(l => l.Language).FirstOrDefault();
            await _firebaseRepository.SendIndividualNotification(new IndividualNotificationModel
            {
                Description = lang == Language.English ? reject.Description
                    : reject.NotificationTranslate.Select(n => n.Description).FirstOrDefault(),
                GenericId = announcement.Id,
                NotificationType = NotificationType.RejectAnnouncement,
                ReceiverId = announcement.UserId,
                SenderId = null,
                Title = lang == Language.English ? reject.Title
                    : reject.NotificationTranslate.Select(n => n.Title).FirstOrDefault()
            }, false);
            return true;
        }

        public async Task<string> Share(int announcementId)
        {
            var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId).Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                x.BasePhoto
            }).FirstOrDefaultAsync();
            if (announcement == null)
                return null;
            var url = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize, UploadType.AnnouncementBasePhoto,
                     announcement.BasePhoto, 500, 500);
            var html = $"https://baitkm.com/products/details/{announcementId}";
            return html;
        }

        public async Task<IEnumerable<RejectTypesModel>> GetAnnouncementRejectsType()
        {
            return await _repository.Filter<Notification>(n => n.NotificationType == AnnouncementNotificationType.RejectReason)
                .Select(x => new RejectTypesModel
                {
                    Id = x.Id,
                    Description = x.Description
                }).ToListAsync();
        }

        public async Task<bool> AddRatingAsync(AddRatingModel model, int userId, string deviceId)
        {
            Guest guest = null;
            Rating rating = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().NoGuest);
            }

            var announcement = await _repository.Filter<Announcement>(a => a.Id == model.AnnouncementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);

            if (user != null)
            {
                rating = await _repository.FilterAsNoTracking<Rating>(r => r.UserId == user.Id
                    && r.AnnouncementId == announcement.Id).FirstOrDefaultAsync();
                if (rating != null)
                    throw new Exception("You already rate for this announcement");
                _repository.Create(new Rating
                {
                    UserId = user.Id,
                    AnnouncementId = announcement.Id,
                    Rat = model.Rating
                });
            }
            else
            {
                rating = await _repository.FilterAsNoTracking<Rating>(r => r.GuestId == guest.Id
                    && r.AnnouncementId == announcement.Id).FirstOrDefaultAsync();
                if (rating != null)
                    throw new Exception("You already rate for this announcement");
                _repository.Create(new Rating
                {
                    GuestId = guest.Id,
                    AnnouncementId = announcement.Id,
                    Rat = model.Rating
                });
            }
            var totalRating = await CalculateCompanyRating(announcement.Id, model.Rating);
            announcement.Rating = totalRating;
            _repository.Update(announcement);

            await _repository.SaveChangesAsync();
            return true;
        }


        //public async Task<bool> ApproveAnnouncement(AddTitleDescriptionModel model, Language language, int announcementId)
        //{
        //    var announcemenIsAvailable = await _repository.Filter<Notification>(n =>
        //         n.NotificationType == AnnouncementNotificationType.Available).Include(n => n.NotificationTranslate).FirstOrDefaultAsync();
        //    var approve = await _repository.Filter<Notification>
        //        (n => n.NotificationType == AnnouncementNotificationType.Approve)
        //        .Include(n => n.NotificationTranslate).FirstOrDefaultAsync();
        //    dynamic property = new ExpandoObject();
        //    var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId)
        //        .FirstOrDefaultAsync();
        //    if (announcement == null)
        //        throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
        //    if (string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Description)
        //        || string.IsNullOrEmpty(model.TitleArabian) || string.IsNullOrEmpty(model.DescriptionArabian))
        //        throw new Exception(_optionsBinder.Error().TitleDesc);
        //    var userLanguage = await _repository.FilterAsNoTrackingAsync<UserSubscriptionAreaAndLanguage>(x => x.UserId == announcement.UserId);
        //    var lang = userLanguage.Select(l => l.Language).FirstOrDefault();
        //    TimeSpan timeSpan;
        //    property.Title = model.Title;
        //    property.Description = model.Description;
        //    property.TitleArabian = model.TitleArabian;
        //    property.DescriptionArabian = model.DescriptionArabian;
        //    property.AnnouncementStatus = AnnouncementStatus.Accepted;
        //    if (announcement.AnnouncementStatus == AnnouncementStatus.Pending)
        //    {
        //        if (model.Day != 0)
        //            property.AnnouncementStatusLastDay = DateTime.UtcNow.AddDays(model.Day);
        //        else
        //            property.AnnouncementStatusLastDay = DateTime.UtcNow.AddDays(model.DefaultDay);
        //        property.AnnouncementApprovedDay = DateTime.UtcNow;
        //    }
        //    else
        //    {
        //        timeSpan = announcement.AnnouncementStatusLastDay - announcement.AnnouncementChangedDay;
        //        if (timeSpan.Days < 0)
        //            property.AnnouncementChangedDay = announcement.AnnouncementChangedDay;
        //        else
        //            property.AnnouncementStatusLastDay = DateTime.UtcNow.AddDays(timeSpan.Days);
        //    }
        //    _repository.Update(announcement, (ExpandoObject)property);
        //    //await _repository.SaveChangesAsync();
        //    if (announcement.AnnouncementStatus == AnnouncementStatus.Accepted)
        //    {
        //        var date = new DateTime(announcement.AnnouncementStatusLastDay.Year, announcement.AnnouncementStatusLastDay.Month,
        //            announcement.AnnouncementStatusLastDay.Day, announcement.AnnouncementStatusLastDay.Hour,
        //            announcement.AnnouncementStatusLastDay.Minute, announcement.AnnouncementStatusLastDay.Second);
        //        var deactivationDayTriggerBuilder = TriggerBuilder.Create()
        //            .WithIdentity($"{nameof(AnnouncementDeactivationDayJob)}Trigger#{AnnouncementDeactivationDayJob.Name}{announcement.Id}")
        //            .StartAt(date);
        //        dynamic deactivationDay = new ExpandoObject();
        //        deactivationDay.announcementId = announcement.Id;
        //        await SchedulerHelper.Schedule<AnnouncementDeactivationDayJob, IJobListener>(new QuartzScheduleModel
        //        {
        //            Name = $"{AnnouncementDeactivationDayJob.Name}{announcement.Id}",
        //            IsListenerRequested = false,
        //            DataMap = deactivationDay,
        //            TriggerBuilder = deactivationDayTriggerBuilder
        //        });
        //        _repository.Create(new PersonNotification
        //        {
        //            NotificationId = approve.Id,
        //            AnnouncementId = announcement.Id,
        //            UserId = announcement.UserId,
        //        });
        //    }
        //    var conversations = await _repository.Filter<Conversation>(x => x.AnnouncementId == announcement.Id).ToListAsync();
        //    foreach (var conversation in conversations)
        //    {
        //        var questionerLanguage = _repository.FilterAsNoTracking<UserSubscriptionAreaAndLanguage>(x =>
        //            x.UserId == conversation.QuestionerId).Select(s => s.Language).FirstOrDefault();
        //        await _firebaseRepository.SendIndividualNotification(new IndividualNotificationModel
        //        {
        //            Description = $"{announcement.Title}",
        //            GenericId = announcement.Id,
        //            NotificationType = NotificationType.AnnouncementAvailable,
        //            ReceiverId = conversation.QuestionerId,
        //            SenderId = null,
        //            Title = questionerLanguage == Language.English ? announcemenIsAvailable.Title
        //           : announcemenIsAvailable.NotificationTranslate.Select(n => n.Title).FirstOrDefault()
        //        }, false);
        //        _repository.Create(new PersonNotification
        //        {
        //            AnnouncementId = announcement.Id,
        //            UserId = conversation.QuestionerId,
        //            NotificationId = announcemenIsAvailable.Id
        //        });
        //    }
        //    await _repository.SaveChangesAsync();
        //    await _firebaseRepository.SendIndividualNotification(new IndividualNotificationModel
        //    {
        //        Description = lang == Language.English ? approve.Description
        //            : approve.NotificationTranslate.Select(n => n.Description).FirstOrDefault(),
        //        GenericId = announcement.Id,
        //        NotificationType = NotificationType.ApprovedAnnouncement,
        //        ReceiverId = announcement.UserId,
        //        SenderId = null,
        //        Title = lang == Language.English ? approve.Title
        //            : approve.NotificationTranslate.Select(n => n.Title).FirstOrDefault(),
        //    }, false);
        //    await NewAnnouncementNotification(announcement);
        //    return true;
        //}

        public async Task<bool> NewAnnouncementNotification(Announcement announcement)
        {
            var newAnnouncement = await _repository.Filter<Notification>(n =>
                n.NotificationType == AnnouncementNotificationType.NewNotification).FirstOrDefaultAsync();
            IQueryable<SaveFilter> userQuery;
            userQuery = _repository.GetAll<SaveFilter>();
            if (userQuery != null)
            {
                userQuery = userQuery.Where(x => x.AnnouncementType == null ? true : x.AnnouncementType == announcement.AnnouncementType);
                userQuery = userQuery.Where(x => x.AnnouncementEstateType == null ? true : x.AnnouncementEstateType == announcement.AnnouncementEstateType);
                if (announcement.AnnouncementType == AnnouncementType.Rent && announcement.AnnouncementRentType != null)
                    userQuery = userQuery.Where(x => x.AnnouncementRentType == announcement.AnnouncementRentType);
                if (announcement.AnnouncementEstateType == AnnouncementEstateType.Residential && announcement.AnnouncementResidentialType != null)
                    userQuery = userQuery.Where(x => x.AnnouncementResidentialType == announcement.AnnouncementResidentialType);
                if (announcement.BathroomCount != 0)
                    userQuery = userQuery.Where(x => x.BathroomCount == announcement.BathroomCount);
                if (announcement.BedroomCount != 0)
                    userQuery = userQuery.Where(x => x.BedroomCount == announcement.BedroomCount);
                if (announcement.Price != 0)
                    userQuery = userQuery.Where(x => x.PriceFrom < announcement.Price && x.PriceTo > announcement.Price);
                if (announcement.Area != 0)
                    userQuery = userQuery.Where(x => x.MinArea < announcement.Area && x.MaxArea > announcement.Area);
                var users = userQuery.Select(x => x.User).Include(y => y.PersonSettings).Distinct().ToList();
                foreach (var user in users)
                {
                    var userLanguage = await _repository.Filter<PersonOtherSetting>(x => x.UserId == user.Id)
                        .Select(x => x.Language).FirstOrDefaultAsync();
                    if (user.PersonSettings.Select(x => x.SubscriptionsType).Contains(SubscriptionsType.NewSavedFilterSuggestionNotifications))
                    {
                        _repository.Create(new PersonNotification
                        {
                            AnnouncementId = announcement.Id,
                            UserId = user.Id,
                            NotificationId = newAnnouncement.Id
                        });
                        await _firebaseRepository.SendIndividualNotification(new IndividualNotificationModel
                        {
                            Description = userLanguage == Language.English ? newAnnouncement.Description :
                                newAnnouncement.NotificationTranslate.Select(n => n.Description)
                                .FirstOrDefault(),
                            GenericId = announcement.Id,
                            NotificationType = NotificationType.NewSavedFilterSuggestionNotifications,
                            ReceiverId = user.Id,
                            SenderId = null,
                            Title = userLanguage == Language.English ? newAnnouncement.Title :
                                newAnnouncement.NotificationTranslate.Select(n => n.Title)
                                .FirstOrDefault(),
                        }, false);
                    }
                }
                await _repository.SaveChangesAsync();
            }
            IQueryable<SaveFilter> guestQuery;
            guestQuery = _repository.GetAll<SaveFilter>();
            if (guestQuery != null)
            {
                guestQuery = guestQuery.Where(x => x.AnnouncementType == null ? true : x.AnnouncementType == announcement.AnnouncementType);
                guestQuery = guestQuery.Where(x => x.AnnouncementEstateType == null ? true : x.AnnouncementEstateType == announcement.AnnouncementEstateType);
                if (announcement.AnnouncementType == AnnouncementType.Rent && announcement.AnnouncementRentType != null)
                    guestQuery = guestQuery.Where(x => x.AnnouncementRentType == announcement.AnnouncementRentType);
                if (announcement.AnnouncementEstateType == AnnouncementEstateType.Residential && announcement.AnnouncementResidentialType != null)
                    guestQuery = guestQuery.Where(x => x.AnnouncementResidentialType == announcement.AnnouncementResidentialType);
                if (announcement.BathroomCount != 0)
                    guestQuery = guestQuery.Where(x => x.BathroomCount == announcement.BathroomCount);
                if (announcement.BedroomCount != 0)
                    guestQuery = guestQuery.Where(x => x.BedroomCount == announcement.BedroomCount);
                if (announcement.Price != 0)
                    guestQuery = guestQuery.Where(x => x.PriceFrom < announcement.Price && x.PriceTo > announcement.Price);
                if (announcement.Area != 0)
                    guestQuery = guestQuery.Where(x => x.MinArea < announcement.Area && x.MaxArea > announcement.Area);
                var guests = guestQuery.Select(x => x.Guest).Include(y => y.PersonSettings).ToList();
                foreach (var guest in guests)
                {
                    var guestLanguage = await _repository.Filter<PersonOtherSetting>(x => x.GuestId == guest.Id)
                        .Select(x => x.Language).FirstOrDefaultAsync();
                    if (guest.PersonSettings.Select(x => x.SubscriptionsType).Contains(SubscriptionsType.NewSavedFilterSuggestionNotifications))
                    {
                        await _firebaseRepository.SendIndividualNotification(new IndividualNotificationModel
                        {
                            Description = guestLanguage == Language.English ? newAnnouncement.Description :
                                newAnnouncement.NotificationTranslate.Select(n => n.Description)
                                .FirstOrDefault(),
                            GenericId = announcement.Id,
                            NotificationType = NotificationType.NewSavedFilterSuggestionNotifications,
                            ReceiverId = guest.Id,
                            SenderId = null,
                            Title = guestLanguage == Language.English ? newAnnouncement.Title :
                                newAnnouncement.NotificationTranslate.Select(n => n.Title)
                                .FirstOrDefault(),
                        }, false);
                    }
                }
            }
            return true;
        }

        public async Task<bool> UpdateExpiredAnnouncement(int announcementId, Language language, string userName)
        {
            var user = await _repository.Filter<User>(x => x.Email == userName || (x.PhoneCode + x.Phone) == userName).FirstOrDefaultAsync();
            if (user != null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId && x.UserId == user.Id).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            if (announcement.AnnouncementStatus != AnnouncementStatus.Expired)
                throw new Exception(_optionsBinder.Error().UpdateExpired);
            announcement.AnnouncementStatus = AnnouncementStatus.Accepted;
            _repository.Update(announcement);
            return true;
        }

        public async Task<bool> DecrementCount(int announcementId)
        {
            ConstValues.ProgressModels.TryGetValue(announcementId, out var existing);
            var announcement = await _repository.Filter<Announcement>(x => x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            if (existing == null)
                return true;
            --existing.Total;
            ConstValues.ProgressModels.TryRemove(announcementId, out _);
            if (existing.Total != existing.Done)
                ConstValues.ProgressModels.TryAdd(announcementId, existing);
            var notify = Utilities.SerializeObject(new ProgressSocketEmitModel
            {
                AnnouncementId = announcementId,
                Increment = false
            });
            await AnnouncementProgressHandler.SendMessageAsync(announcement.UserId, notify);
            return true;
        }

        public async Task<bool> FilesCount(int announcementId, int count)
        {
            var announcement = await _repository.Filter<Announcement>(x => x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            ConstValues.ProgressModels.TryAdd(announcementId, new ProgressHelperModel
            {
                Total = count,
                Done = 0
            });
            return true;
        }



        private async Task<double> CalculateCompanyRating(int announcementId, int rating)
        {
            var announcementRatings = await _repository.FilterAsNoTracking<Rating>(a => a.Id == announcementId)
                .Select(c => c.Rat).ToListAsync();
            double totalRating = 0.0f;
            foreach (var announcementRating in announcementRatings)
            {
                totalRating += announcementRating;
            }
            totalRating += rating;
            return totalRating / (announcementRatings.Count() + 1);
        }

        private static double DistanceTo(double lat1, double lng1, double lat2, double lng2, char unit = 'M')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lng1 - lng2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }
    }
}