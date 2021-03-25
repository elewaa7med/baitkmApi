using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class EditAnnouncementModel : IViewModel
    {
        public AnnouncementType? AnnouncementType { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        public AnnouncementEstateType? AnnouncementEstateType { get; set; }
        public AnnouncementResidentialType? AnnouncementResidentialType { get; set; }
        public AnnouncementStatus? AnnouncementStatus { get; set; }
        public List<AnnouncementFeaturesType> Features { get; set; }
        public string Address { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public decimal? Price { get; set; }
        public decimal? Area { get; set; }
        public int? BedroomCount { get; set; }
        public int? BathroomCount { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TitleArabian { get; set; }
        public string DescriptionArabian { get; set; }
        public bool IsOtherPeriod { get; set; }
        public decimal? LivingArea { get; set; }
        public int? SittingCount { get; set; }
        public decimal? KitchenArea { get; set; }
        public decimal? BalconyArea { get; set; }
        public decimal? LaundryArea { get; set; }
        public int Floor { get; set; }
        public ConstructionStatus? ConstructionStatus { get; set; }
        public SaleType? SaleType { get; set; }
        public FurnishingStatus? FurnishingStatus { get; set; }
        public OwnerShip? OwnerShip { get; set; }
        public BuildingAge? BuildingAge { get; set; }
        public int? CurrencyId { get; set; }
        public CommercialType? CommercialType { get; set; }
        public LandType? LandType { get; set; }
        public FacadeType? FacadeType { get; set; }
        public int? LandNumber { get; set; }
        public int? PlanNumber { get; set; }
        public string DisctrictName { get; set; }
        public decimal? StreetWidth { get; set; }
        public int? NumberOfAppartment { get; set; }
        public int? NumberOfFloors { get; set; }
        public int? NumberOfVilla { get; set; }
        public int? NumberOfShop { get; set; }
        public bool FireSystem { get; set; }
        public bool OfficeSpace { get; set; }
        public bool LaborResidence { get; set; }
        public string District { get; set; }
        public int? NumberOfWareHouse { get; set; }
        public int? NumberOfUnits { get; set; }
        public LandCategory? LandCategory { get; set; }
    }
}
