using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class MapFilterAnnouncementModel : IViewModel
    {
        public AnnouncementType? AnnouncementType { get; set; }
        public AnnouncementEstateType? AnnouncementEstateType { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        public List<AnnouncementFeaturesType> Features { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public int? BedroomCount { get; set; }
        public int? BathroomCount { get; set; }
        public decimal? MinArea { get; set; }
        public decimal? MaxArea { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public decimal Distance { get; set; }

        ///////////////////////////////////////////////////////////
        public string Address { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? SittingCount { get; set; }
        public string UserName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public ConstructionStatus? ConstructionStatus { get; set; }
        public SaleType? SaleType { get; set; }
        public FurnishingStatus? FurnishingStatus { get; set; }
        public OwnerShip? OwnerShip { get; set; }
        public BuildingAge? BuildingAge { get; set; }
        public CommercialType? CommercialType { get; set; }
        public LandType? LandType { get; set; }
        public FacadeType? FacadeType { get; set; }
        public decimal? MinMeterPrice { get; set; }
        public decimal? MaxMeterPrice { get; set; }
        public string DisctrictName { get; set; }
    }
}
