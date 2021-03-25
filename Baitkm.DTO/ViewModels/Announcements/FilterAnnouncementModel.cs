using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class FilterAnnouncementModel : IViewModel
    {
        public string Search { get; set; }//in admin, mobile// for title and id
        public int? CountryId { get; set; }//in admin, mobile
        public int? CityId { get; set; } //in admin, mobile
        public string Address { get; set; } //check//in map
        public AnnouncementType? AnnouncementType { get; set; }//in admin, mobile
        public AnnouncementEstateType? AnnouncementEstateType { get; set; }//in admin, mobile
        public SaleType? SaleType { get; set; }//in admin, mobile
        public List<AnnouncementFeaturesType> Features { get; set; }//in admin, mobile
        public decimal? PriceFrom { get; set; }//in admin, mobile
        public decimal? PriceTo { get; set; }//in admin, mobile
        public decimal? MinArea { get; set; }//in admin, mobile
        public decimal? MaxArea { get; set; }//in admin, mobile
        public FurnishingStatus? FurnishingStatus { get; set; }// in admin, mobile

        public AnnouncementResidentialType? AnnouncementResidentialType { get; set; }//in admin, mobile
        public AnnouncementRentType? AnnouncementRentType { get; set; }//in admin, mobile
        public CommercialType? CommercialType { get; set; }// in admin, mobile
        public LandType? LandType { get; set; }// in admin, mobile
        public FacadeType? FacadeType { get; set; }// in admin, mobile
        public int? BedroomCount { get; set; } // in admin, mobile
        public int? BathroomCount { get; set; }// in admin, mobile


        //incorect key name
        public int? AnnouncementiId { get; set; } // check // in admin
        public string UserName { get; set; } //check//in admin
        public DateTime? DateFrom { get; set; }//check//in admin
        public DateTime? DateTo { get; set; } //check//in admin
        public ConstructionStatus? ConstructionStatus { get; set; }//check // in admin
        public OwnerShip? OwnerShip { get; set; }//check //for admin
        public BuildingAge? BuildingAge { get; set; }//check //in admin

        public SortingType? SortingType { get; set; }//in admin
        public AnnouncementStatus? AnnouncementStatus { get; set; }//check//use in admin


        public decimal? Lat { get; set; } //check//chka
        public decimal? Lng { get; set; } //check//chka
        public int? SittingCount { get; set; }//check//chka


        public string DisctrictName { get; set; } // check
        public decimal? MinMeterPrice { get; set; }// check//chka
        public decimal? MaxMeterPrice { get; set; }// check//chka
        public LandCategory? LandCategory { get; set; } //check/ not exist when type is land
    }
}
