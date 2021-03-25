using Baitkm.Entities.Base;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class SaveFilter : EntityBase
    {
        public int? UserId { get; set; }
        public int? GuestId { get; set; }

        public int FilterCount { get; set; }
        public string SaveFilterName { get; set; }
        public string Description { get; set; }
        public string Search { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public AnnouncementType? AnnouncementType { get; set; }
        public AnnouncementEstateType? AnnouncementEstateType { get; set; }
        public AnnouncementResidentialType? AnnouncementResidentialType { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        public LandType? LandType { get; set; }
        public FurnishingStatus? FurnishingStatus { get; set; }
        public LandCategory? LandCategory { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public int? BedroomCount { get; set; }
        public int? BathroomCount { get; set; }
        public decimal? MinArea { get; set; }
        public decimal? MaxArea { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public SaleType? SaleType { get; set; }
        public BuildingAge? BuildingAge { get; set; }
        public ConstructionStatus? ConstructionStatus { get; set; }
        public CommercialType? CommercialType { get; set; }
        public FacadeType? FacadeType { get; set; }
        public int? SittingCount { get; set; }
        public OwnerShip? OwnerShip { get; set; }
        public decimal? MinMeterPrice { get; set; }
        public decimal? MaxMeterPrice { get; set; }

        public virtual User User { get; set; }
        public virtual Guest Guest { get; set; }
        public virtual ICollection<SaveFilterFeature> Features { get; set; }
    }
}