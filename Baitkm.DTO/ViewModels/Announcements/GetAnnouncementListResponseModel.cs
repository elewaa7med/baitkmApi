using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class GetAnnouncementListResponseModel : IViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementEstateType AnnouncementEstateType { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        public AnnouncementResidentialType? AnnouncementResidentialType { get; set; }
        public AnnouncementStatus AnnouncementStatus { get; set; }
        public string Address { get; set; }
        public decimal Price { get; set; }
        public long Area { get; set; }
        public int? BedroomCount { get; set; }
        public int? BathroomCount { get; set; }
        public ImageOptimizer Photo { get; set; }
        public bool IsFavourite { get; set; }
        public string ShareUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public IEnumerable<ImageAndVideoOptimizer> Photos { get; set; }
        public CommercialType? CommercialType { get; set; }
        public LandType? LandType { get; set; }
        public double Rating { get; set; }
    }
}