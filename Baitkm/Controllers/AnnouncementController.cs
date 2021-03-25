using Baitkm.Authorizations.Admins;
using Baitkm.Authorizations.UserGuests;
using Baitkm.Authorizations.Users;
using Baitkm.BLL.Services.Announcements;
using Baitkm.BLL.Services.Configurations;
using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    //[Authorize]
    public class AnnouncementController : BaseController
    {
        private readonly IAnnouncementService _service;

        public AnnouncementController(IAnnouncementService service, IConfigurationService configurationService)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> Add([FromBody] AddAnnouncementModel model)
        {
            return await MakeActionCallAsync(async () => await _service.AddAsync(model, GetPersonId(), GetLanguage()));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> Edit([FromBody] EditAnnouncementModel model, int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.EditAsync(model, GetPersonId(), GetLanguage(), announcementId));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [RequestSizeLimit(6000000000)]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> UploadAnnouncementBasePhoto([FromForm] UploadFileModel model, int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.UploadBasePhoto(model, announcementId));
        }

        /// <summary>
        /// this endpoint for refactoring
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> test_MapUpload([FromQuery] decimal lat, decimal lng, int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.UploadMap(lat, lng, announcementId));
        }
        //[HttpDelete]
        //[Route("{announcementId}")]
        //public async Task<IActionResult> Delete(int announcementId)
        //{
        //    return await MakeActionCallAsync(async () => await _service.Delete(announcementId, GetPerson(), GetVerifiedBy()));
        //}

        [HttpPut]
        [RequestSizeLimit(6000000000)]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> UploadOtherPhotos([FromForm] MultipleUpload model)
        {
            return await MakeActionCallAsync(async () => await _service.UploadOtherPhotos(model));
        }

        [HttpPut]
        [RequestSizeLimit(6000000000)]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> UploadOtherDocumentation([FromForm] MultipleUpload model)
        {
            return await MakeActionCallAsync(async () => await _service.UploadOtherDocumentation(model));
        }

        [HttpPost]
        [Route("{path}")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string path)
        {
            HttpContext.Request.Headers.TryGetValue("announcementId", out var announcementIdValues);
            HttpContext.Request.Headers.TryGetValue("announcementPhotoType", out var announcementPhotoTypeValues);
            int.TryParse(announcementIdValues.FirstOrDefault(), out var announcementId);
            System.Enum.TryParse(announcementPhotoTypeValues.FirstOrDefault(), out AttachmentType announcementPhotoType);
            return await MakeActionCallAsync(async () => await _service.UploadCallback(path, announcementId, announcementPhotoType));
        }

        [HttpGet]
        [Route("{announcementId}")]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> MyAnnouncementDetails(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.GetMyAnnouncementDetails(announcementId, GetPersonId(),
                GetLanguage(), GetDeviceId()));
        }

        [HttpGet]
        [Route("{announcementId}")]
        [UserAuthorize]
        public async Task<IActionResult> MyAnnouncementDetailsForEdit(int announcementId)
        {
            return await MakeActionCallAsync(async () =>
                await _service.GetMyAnnouncementDetailsForEdit(GetPersonId(), announcementId, GetLanguage()));
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> MyAnnouncementList([FromBody] MyAnnouncementListModel model)
        {
            return await MakeActionCallAsync(async () => await _service.MyAnnouncementList(model, GetPersonId(), GetLanguage()));
        }

        //TODO Delete - when mobile not call this api
        [HttpPost]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> MyAnnouncementListMobile([FromBody] MyAnnouncementListModel model)
        {
            return await MakeActionCallAsync(async () => await _service.MyAnnouncementListMobile(model, GetPersonId(), GetLanguage()));
        }

        [HttpPost]
        [Route("{userId}")]
        [UserAuthorize]
        public async Task<IActionResult> MyAnnouncementListByUserId([FromBody] PagingRequestModel model, int userId)
        {
            return await MakeActionCallAsync(async () => await _service.MyAnnouncementListByUserId(model, userId, GetLanguage()));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> Favourite(int announcementId)
        {
            return await MakeActionCallAsync(async () =>
                await _service.FavouriteAsync(announcementId, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> UnFavourite(int announcementId)
        {
            return await MakeActionCallAsync(async () =>
                await _service.UnFavouriteAsync(announcementId, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        [HttpGet]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> FavouriteList([FromQuery] SortingType sortingType)
        {
            return await MakeActionCallAsync(async () =>
                await _service.FavouriteListAsync(GetPersonId(), GetDeviceId(), GetLanguage(), sortingType));
        }

        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> FeaturedList([FromBody] PagingRequestModel model, [FromQuery] SortingType sortingType)
        {
            return await MakeActionCallAsync(async () =>
                await _service.FeaturedList(model, GetPersonId(), GetDeviceId(), sortingType, GetLanguage()));
        }

        //TODO Delete - when mobile not call this api
        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> FeaturedListMobile([FromBody] PagingRequestModel model, [FromQuery] SortingType sortingType)
        {
            return await MakeActionCallAsync(async () =>
                await _service.FeaturedListMobile(model, GetPersonId(), GetDeviceId(), sortingType, GetLanguage()));
        }

        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> Suggesting([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () =>
                await _service.SuggestingAsync(model, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        //TODO Delete - when mobile not call this api
        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> SuggestingMobile([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () =>
                await _service.SuggestingMobile(model, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        [HttpPost]
        [Route("{announcementId}")]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> SimilarAnnouncement([FromBody] PagingRequestModel model, int announcementId)
        {
            return await MakeActionCallAsync(async () =>
                await _service.SimilarAnnouncementAsync(model, announcementId, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        //TODO Delete - when mobile not call this api
        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> NearbyMobile([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.NearbyMobileAsync(model,
                GetRequesterLatLng().Item1, GetRequesterLatLng().Item2, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> Nearby([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.NearbyAsync(model,
                GetRequesterLatLng().Item1, GetRequesterLatLng().Item2, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> AnnouncementFilter([FromBody] PagingRequestAnnouncementFilterModel model)
        {
            return await MakeActionCallAsync(async () =>
                await _service.AnnouncementFilterAsync(model, GetPersonId(), GetDeviceId(), GetLanguage()));
        }

        /// <summary>
        /// Return announcement response Count
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public IActionResult AnnouncementFilterCount([FromBody] FilterAnnouncementModel model)
        {
            return MakeActionCall(() => _service.AnnouncementFilterCount(model, GetPersonId(), GetLanguage(), GetDeviceId()));
        }

        /// <summary>
        /// This api use only android, return Choose property count
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public IActionResult FilterCount([FromBody] FilterAnnouncementModel model)
        {
            return MakeActionCall(() => _service.AnnouncementFilterPropertiesCount(model));
        }

        [HttpPost]
        [AllowAnonymous]
        [UserGuestAuthorize]
        public async Task<IActionResult> MapSmallRadius([FromBody] MapFilterAnnouncementModel model)
        {
            return await MakeActionCallAsync(async () => await _service.MapSmallRadius(model, GetPersonId(), GetLanguage(), GetDeviceId()));
        }
        
        [HttpGet]
        [AdminAuthorize]
        public async Task<IActionResult> DashboardPendingListAdmin()
        {
            return await MakeActionCallAsync(async () => await _service.DashboardPendingListAdmin(GetPersonId()));
        }

        [HttpGet]
        [AdminAuthorize]
        public IActionResult DashboardStatistic()
        {
            return MakeActionCall(() => _service.DashboardStatistic());
        }

        [HttpPost]
        [AdminAuthorize]
        public IActionResult DashboardAnnouncementStatistic([FromBody] GetDashboardAnnouncementStatisticRequestModel model)
        {
            return MakeActionCall(() => _service.DashboardAnnouncementStatistic(GetPersonId(), model));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> AdminAnnouncementList([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetAnnouncementListAsync(model, GetPersonId()));
        }

        [HttpPut]
        [AdminAuthorize]
        [Route("{announcementId}")]
        public async Task<IActionResult> AddToTopList([FromBody] AddToTopListModel model, int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.AddToTopListAsync(model, GetLanguage(), announcementId));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> AnnouncementReportList([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.AnnouncementReportListAsync(model, UserCurrency, GetLanguage()));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> ReportFilter([FromBody] PagingRequestReportFilterModel model)
        {
            return await MakeActionCallAsync(async () => await _service.ReportFilter(model, GetLanguage()));
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> AnnouncementReportAdd([FromBody] AnnouncementReportAddModel model)
        {
            return await MakeActionCallAsync(async () => await _service.AddReportAsync(model, GetPersonId()));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [AdminAuthorize]
        public async Task<IActionResult> ApproveReport(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.ApproveReportAsync(announcementId));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [AdminAuthorize]
        public async Task<IActionResult> RejectReport(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.RejectReportAsync(announcementId));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> HideAnnouncement(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.HideAnnouncementAsync(announcementId, GetPersonId()));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> MakeActiveAnnouncement(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.MakeActiveAnnouncementAsync(announcementId, GetPersonId()));
        }

        [HttpPut]
        [Route("{announcementId}")]
        [AdminAuthorize]
        public async Task<IActionResult> RejectAnnouncement([FromBody] AnnouncementRejectModel model, int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.RejectAnnouncement(model, GetLanguage(), announcementId));
        }

        //[HttpPut]
        //[Route("{announcementId}")]
        //[Authorize(Roles = Roles.Admin)]
        //public async Task<IActionResult> ApproveAnnouncement([FromBody]AddTitleDescriptionModel model, int announcementId)
        //{
        //    return await MakeActionCallAsync(async () => await _service.ApproveAnnouncement(model, GetLanguage(), announcementId));
        //}

        [HttpGet]
        [AllowAnonymous]
        [Route("{announcementId}")]
        public async Task<IActionResult> Share(int announcementId)
        {
            var content = await _service.Share(announcementId);
            return new ContentResult
            {
                Content = content,
                ContentType = "text/html"
            };
        }

        [HttpGet]
        [AdminAuthorize]
        public async Task<IActionResult> GetAnnouncementRejectsType()
        {
            return await MakeActionCallAsync(async () => await _service.GetAnnouncementRejectsType());
        }

        [HttpPost]
        [UserGuestAuthorize]
        public async Task<IActionResult> Rating([FromBody] AddRatingModel model)
        {
            return await MakeActionCallAsync(async () => await _service.AddRatingAsync(model, GetPersonId(), GetDeviceId()));
        }

        //TO DO - check for what is it 
        [HttpPut]
        [Route("{announcementId}")]
        public async Task<IActionResult> UpdateExpiredAnnouncement(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.UpdateExpiredAnnouncement(announcementId, GetLanguage(), GetPerson()));
        }

        //TO DO - check for what is it 
        [HttpGet]
        [Route("{announcementId}")]
        public async Task<IActionResult> DecrementCount(int announcementId)
        {
            return await MakeActionCallAsync(async () => await _service.DecrementCount(announcementId));
        }

        //TO DO - check for what is it 
        [HttpPost]
        [Route("{announcementId}/{count}")]
        [AllowAnonymous]
        public async Task<IActionResult> FilesCount(int announcementId, int count)
        {
            return await MakeActionCallAsync(async () => await _service.FilesCount(announcementId, count));
        }
    }
}