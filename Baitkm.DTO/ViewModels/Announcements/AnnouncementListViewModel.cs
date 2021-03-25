using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class AnnouncementListViewModel : IViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TitleArabian { get; set; }
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
        public ImageOptimizer UserProfilePhoto { get; set; }
        public List<ImageAndVideoOptimizer> Photos { get; set; }
        public bool IsFavourite { get; set; }
        public string ShareUrl { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime CreateDate { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
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
        public CommercialType? CommercialType { get; set; }
        public LandType? LandType { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int CityId { get; set; }
        public FacadeType? FacadeType { get; set; }
        public int? LandNumber { get; set; }
        public int? PlanNumber { get; set; }
        public string DisctrictName { get; set; }
        public decimal? MeterPrice { get; set; }
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
        public DateTime CreatedDt { get; set; }
        public int? NumberOfUnits { get; set; }
        public LandCategory? LandCategory { get; set; }
        public double Rating { get; set; }
        public int View { get; set; }
    }
}