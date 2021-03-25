using Baitkm.Entities.Base;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class Announcement : EntityBase
    {
        public AnnouncementEstateType AnnouncementEstateType { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementStatus AnnouncementStatus { get; set; }
        public AnnouncementResidentialType? AnnouncementResidentialType { get; set; }
        public decimal Area { get; set; }
        public decimal? LivingArea { get; set; }
        public int? SittingCount { get; set; }
        public decimal? KitchenArea { get; set; }
        public decimal? BalconyArea { get; set; }
        public decimal? LaundryArea { get; set; }
        public decimal Price { get; set; }
        public int BedroomCount { get; set; } // ?
        public int BathroomCount { get; set; }// ?
        public string Title { get; set; }
        public string Description { get; set; }
        public string TitleArabian { get; set; }
        public string DescriptionArabian { get; set; }
        public int UserId { get; set; }


        public string AddressEn { get; set; }
        public string AddressAr { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public int CityId { get; set; }
        public int CountryId { get; set; }
        public string BasePhoto { get; set; }
        public DateTime AnnouncementStatusLastDay { get; set; }
        public DateTime AnnouncementApprovedDay { get; set; }
        public DateTime AnnouncementChangedDay { get; set; }
        public bool TopAnnouncement { get; set; }
        public DateTime TopAnnouncementDayFrom { get; set; }
        public DateTime TopAnnouncementDayTo { get; set; }
        public bool IsOtherPeriod { get; set; }
        public bool IsDraft { get; set; }
        public int ViewsCount { get; set; }
        public int Floor { get; set; }
        public ConstructionStatus? ConstructionStatus { get; set; }
        public SaleType? SaleType { get; set; }
        public FurnishingStatus? FurnishingStatus { get; set; }
        public OwnerShip? OwnerShip { get; set; }
        public BuildingAge? BuildingAge { get; set; }
        public int CurrencyId { get; set; }
        public CommercialType? CommercialType { get; set; }
        public LandType? LandType { get; set; }
        public FacadeType? FacadeType { get; set; }
        public int? LandNumber { get; set; }
        public int? PlanNumber { get; set; }
        public string DisctrictName { get; set; }
        public decimal? MeterPrice { get; set; }
        public decimal? StreetWidth { get; set; }
        public int? NumberOfAppartment { get; set; }
        public int? NumberOfUnits { get; set; }
        public int? NumberOfFloors { get; set; }
        public int? NumberOfVilla { get; set; }
        public int? NumberOfShop { get; set; }
        public bool FireSystem { get; set; }
        public bool OfficeSpace { get; set; }
        public bool LaborResidence { get; set; }
        public string District { get; set; }
        public int? NumberOfWareHouse { get; set; }
        public LandCategory? LandCategory { get; set; }
        public double Rating { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<Favourite> Favourites { get; set; }
        public virtual ICollection<AnnouncementFeature> Features { get; set; }
        public virtual ICollection<Conversation> Conversations { get; set; }
        public virtual ICollection<AnnouncementReport> AnnouncementReports { get; set; }
        public virtual ICollection<PersonNotification> PersonNotifications { get; set; }
        public virtual ICollection<ViewedAnnouncement> ViewedAnnouncements { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<SubscribeAnnouncement> SubscribeAnnouncements { get; set; }
        public virtual ICollection<Fact> Facts { get; set; }
    }
}