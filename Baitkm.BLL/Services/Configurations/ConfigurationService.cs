using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Configurations;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Configurations
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IEntityRepository _repository;
        private readonly MediaAccessor _accessor;
        private readonly IOptionsBinder _optionsBinder;
        private readonly IOptions<NgrokSettings> ngrokSettings;
        private readonly IOptions<Currency> currencies;
        private readonly IOptions<CurrnecySymbols> currencySymbols;
        private readonly IOptions<Dictionary<string, int>> tert;
        private readonly IOptions<TestDic> test;
        //private readonly IImageHandler imageHandler;

        public ConfigurationService(IEntityRepository repository,
            IOptionsBinder optionsBinder,
            IOptions<NgrokSettings> ngrokSettings,
            IOptions<Currency> currencies,
            IOptions<CurrnecySymbols> currencySymbols,
            IOptions<Dictionary<string, int>> tert,
            IOptions<TestDic> test
            //IImageHandler imageHandler
            )
        {
            _repository = repository;
            _accessor = new MediaAccessor();
            _optionsBinder = optionsBinder;
            this.ngrokSettings = ngrokSettings;
            this.currencies = currencies;
            this.currencySymbols = currencySymbols;
            this.tert = tert;
            this.test = test;
            //this.imageHandler = imageHandler;
        }

        public Currency Currencies() => currencies.Value;

        public CurrnecySymbols CurrnecySymbols() => currencySymbols.Value;


        public TestDic TestDic() => test.Value;

        public void Tes()
        {
            var er = new Dictionary<string, int>();
            er = tert.Value;
        }

        public async Task<bool> Edit(List<ConfigurationViewModel> model)
        {
            var configurations = await _repository.Filter<Configuration>(x => model.Select(m => m.Id).Contains(x.Id)).ToListAsync();
            foreach (var config in configurations)
            {
                dynamic property = new ExpandoObject();
                foreach (var item in model)
                {
                    if (config.Key == item.Key)
                    {
                        config.Key = item.Key;
                        config.Value = item.Value;
                    }
                }
                _repository.Update(config);
            }
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var configuration = _repository.GetById<Configuration>(id);
            if (configuration == null)
                throw new Exception("configuration not found");
            _repository.Remove(configuration);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<List<ConfigurationViewModel>> GetSettings()
        {
            return await _repository.GetAll<Configuration>()
                .Select(s => new ConfigurationViewModel
                {
                    Id = s.Id,
                    Key = s.Key,
                    Value = s.Value
                }).ToListAsync();
        }

        public async Task<RulesViewModel> GetRules(Language language)
        {
            var result = new RulesViewModel();
            var query = _repository.FilterAsNoTracking<Configuration>(x => true);

            result.PrivacyPolicy = await query.Where(s => s.Language == language && s.Key.Contains("privacypolicy")).Select(x => x.Value).FirstOrDefaultAsync();
            result.TermsAndConditions = await query.Where(s => s.Language == language && s.Key.Contains("termsandconditions")).Select(x => x.Value).FirstOrDefaultAsync();

            return result;
        }

        //public async Task<string> UploadHomePageCoverImage(UploadFileModel model)
        public async Task<ImageOptimizer> UploadHomePageCoverImage(UploadFileModel model)// oldVersion
        {
            var result = await _accessor.Upload(model.File, UploadType.HomePageCoverImage);// oldVersion
            //var result = await imageHandler.UploadImageAsync(model.Photo, UploadType.HomePageCoverImage); // new Version
            var homePageImage = await _repository.FilterAsNoTracking<HomePageCoverImage>(x => !x.IsDeleted).ToListAsync();
            if (!homePageImage.Any())
                _repository.Create(new HomePageCoverImage
                {
                    //Photo = result.Photo,
                    Photo = Path.GetFileName(result),//oldVersion
                    IsBase = true
                });
            else
                _repository.Create(new HomePageCoverImage
                {
                    //Photo = result.Photo,
                    Photo = Path.GetFileName(result),//oldVersion
                    IsBase = false
                });
            await _repository.SaveChangesAsync();
            return new ImageOptimizer //oldVersion
            {
                Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.HomePageCoverImage, result, ConstValues.Width, ConstValues.Height),
                PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.HomePageCoverImage, result, 100, 100, true)
            };
            //return ConstValues.Url + Constants.FileFolder + Constants.HomePageCoverImage + result.Photo;
        }

        public async Task<bool> RemovePhoto(Language language, int id)
        {
            var homePageImages = await _repository.GetAll<HomePageCoverImage>()
                .OrderByDescending(x => x.CreatedDt).ToListAsync();
            var homePageImage = await _repository.FilterAsNoTracking<HomePageCoverImage>(x => x.Id == id).FirstOrDefaultAsync();
            if (homePageImage == null)
                throw new Exception("configuration not found");
            if (homePageImages.Count == 1)
                throw new Exception(_optionsBinder.Error().UploadAnotherPhoto);
            await _accessor.Remove(homePageImage.Photo, UploadType.HomePageCoverImage);
            if (homePageImage.IsBase)
            {
                _repository.HardDelete(homePageImage);
                var lastOrDefault = homePageImages.FirstOrDefault(x => !x.IsBase);
                lastOrDefault.IsBase = true;
                _repository.Update(lastOrDefault);
                return true;
            }
            _repository.HardDelete(homePageImage);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<List<GetHomePageListModel>> GetHomePageCoverImageList()
        {
            return await _repository.GetAll<HomePageCoverImage>().Select(x => new GetHomePageListModel
            {
                Id = x.Id,
                IsBase = x.IsBase,
                //Photo = ConstValues.Url + Constants.FileFolder + Constants.HomePageCoverImage + x.Photo
                //oldVersion 
                Photo = x.Photo.Select(y => new ImageOptimizer
                {
                    Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.HomePageCoverImage, x.Photo, ConstValues.Width, ConstValues.Height, false, 0),
                    PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.HomePageCoverImage, x.Photo, 100, 100, true, 0)
                }).FirstOrDefault()
            }).ToListAsync();
        }

        public async Task<bool> BasePhoto(Language language, int id)
        {
            var homeOldBasePhoto = await _repository.FilterAsNoTracking<HomePageCoverImage>(x => x.IsBase).FirstOrDefaultAsync();
            var homeNewBasePhoto = await _repository.FilterAsNoTracking<HomePageCoverImage>(x => x.Id == id).FirstOrDefaultAsync();
            if (homeNewBasePhoto == null)
                throw new Exception(_optionsBinder.Error().PhotoNotFound);
            dynamic property = new ExpandoObject();
            if (homeOldBasePhoto.Id == homeNewBasePhoto.Id)
                throw new Exception(_optionsBinder.Error().AlreadyCover);
            if (homeOldBasePhoto != null)
                homeOldBasePhoto.IsBase = false;
            homeOldBasePhoto.IsBase = true;
            _repository.Update(homeNewBasePhoto);
            await _repository.SaveChangesAsync();
            return true;
        }

        //public async Task<string> GetBasePhoto()
        public async Task<ImageOptimizer> GetBasePhoto() //oldVersion
        {
            //oldVersion
            return await _repository.FilterAsNoTracking<HomePageCoverImage>(x => x.IsBase).Select(x => new ImageOptimizer
            {
                Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.HomePageCoverImage, x.Photo, 2000, 2000, false, 0),
                PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.HomePageCoverImage, x.Photo, 100, 100, true, 0)
            }).FirstOrDefaultAsync();
            //return await _repository.FilterAsNoTracking<HomePageCoverImage>(x => x.IsBase)
            //    .Select(x => ConstValues.Url + Constants.FileFolder + Constants.HomePageCoverImage + x.Photo).FirstOrDefaultAsync();
        }

        public NgrokSettings GetNgrokSettings() => ngrokSettings.Value;
    }
}