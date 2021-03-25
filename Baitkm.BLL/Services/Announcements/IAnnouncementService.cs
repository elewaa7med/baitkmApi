using Baitkm.DTO.ViewModels.Admin;
using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Persons.Users.CommonModels;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Announcements
{
    public interface IAnnouncementService
    {

        Task<int> AddAsync(AddAnnouncementModel model, int userId, Language language);
        Task<bool> EditAsync(EditAnnouncementModel model, int userId, Language language, int announcementId);
        Task<bool> UploadMap(decimal lat, decimal lng, int announcementId);
        Task<ImageOptimizer> UploadBasePhoto(UploadFileModel model, int announcementId);
        Task<bool> UploadOtherPhotos(MultipleUpload model);
        Task<bool> UploadOtherDocumentation(MultipleUpload model);
        Task<bool> UploadCallback(string path, int announcementId, AttachmentType announcementPhotoType);
        Task<AnnouncementViewModel> GetMyAnnouncementDetails(int announcementId, int userId, Language language, string deviceId);
        Task<MyAnnouncementDetailsForEditModel> GetMyAnnouncementDetailsForEdit(
            int userId, int announcementId, Language language);
        Task<PagingResponseModel<AnnouncementViewModel>> MyAnnouncementList(MyAnnouncementListModel model, int userId, Language language);
        Task<PagingResponseModel<AnnouncementViewModel>> MyAnnouncementListMobile(MyAnnouncementListModel model,
           int userId, Language language);
        Task<PagingResponseModel<AnnouncementViewModel>> MyAnnouncementListByUserId(PagingRequestModel model, int userId, Language language);
        Task<bool> FavouriteAsync(int announcementId, int userId, string deviceId, Language language);
        Task<bool> UnFavouriteAsync(int announcementId, int userId, string deviceId, Language language);
        Task<IEnumerable<AnnouncementViewModel>> FavouriteListAsync(int userId, string deviceId, Language language, SortingType sortingType);
        Task<PagingResponseModel<AnnouncementListViewModel>> FeaturedList(PagingRequestModel model, int userId,
            string deviceId, SortingType sortingType, Language language);
        Task<PagingResponseModel<AnnouncementListViewModel>> FeaturedListMobile(PagingRequestModel model,
            int userId, string deviceId, SortingType sortingType, Language language);
        Task<PagingResponseForSuggesting<AnnouncementViewModel>> SuggestingAsync(PagingRequestModel model,
            int userId, string deviceId, Language language);
        Task<PagingResponseForSuggesting<AnnouncementViewModel>> SuggestingMobile(PagingRequestModel model,
           int userId, string deviceId, Language language);
        Task<PagingResponseModel<AnnouncementViewModel>> SimilarAnnouncementAsync(PagingRequestModel model, int announcementId,
            int userId, string deviceId, Language language);
        Task<PagingResponseForSuggesting<AnnouncementViewModel>> NearbyMobileAsync(PagingRequestModel model,
            decimal lat, decimal lng, int userId, string deviceId, Language language);
        Task<PagingResponseForSuggesting<AnnouncementViewModel>> NearbyAsync(PagingRequestModel model,
            decimal lat, decimal lng, int userId, string deviceId, Language language);
        Task<PagingResponseAnnouncementFilter> AnnouncementFilterAsync(PagingRequestAnnouncementFilterModel model,
            int userId, string deviceId, Language language);
        int AnnouncementFilterPropertiesCount(FilterAnnouncementModel model);
        int AnnouncementFilterCount(FilterAnnouncementModel model, int userId, Language language, string deviceId);
        Task<IEnumerable<AnnouncementViewModel>> MapSmallRadius(MapFilterAnnouncementModel model, int userId, Language language, string deviceId);
        Task<DashboardViewModel> DashboardPendingListAdmin(int userId);
        List<StatisticViewModel> DashboardStatistic();
        GetDashboardAnnouncementStatisticResponseModel DashboardAnnouncementStatistic
            (int userId, GetDashboardAnnouncementStatisticRequestModel model);
        Task<PagingResponseModel<AnnouncementViewModel>> GetAnnouncementListAsync(PagingRequestModel model, int userId);
        Task<bool> AddToTopListAsync(AddToTopListModel model, Language language, int announcementId);
        Task<PagingResponseModel<AnnouncementReportModel>> AnnouncementReportListAsync(PagingRequestModel model, string userCurrency, Language language);
        Task<PagingResponseReportFilter> ReportFilter(PagingRequestReportFilterModel model, Language language);
        Task<bool> AddReportAsync(AnnouncementReportAddModel model, int userId);
        Task<bool> ApproveReportAsync(int announcementId);
        Task<bool> RejectReportAsync(int announcementId);
        Task<bool> HideAnnouncementAsync(int announcementId, int userId);
        Task<bool> MakeActiveAnnouncementAsync(int announcementId, int userId);
        Task<bool> RejectAnnouncement(AnnouncementRejectModel model, Language language, int announcementId);
        Task<string> Share(int announcementId);
        Task<IEnumerable<RejectTypesModel>> GetAnnouncementRejectsType();
        Task<bool> AddRatingAsync(AddRatingModel model, int userId, string deviceId);
        Task<bool> UpdateExpiredAnnouncement(int announcementId, Language language, string userName);
        Task<bool> DecrementCount(int announcementId);
        Task<bool> FilesCount(int announcementId, int count);
        //Task<bool> Delete(int announcementId, string userName, VerifiedBy verifiedBy);//Will test
        //Task<bool> ApproveAnnouncement(AddTitleDescriptionModel model, Language language, int announcementId);
    }
}