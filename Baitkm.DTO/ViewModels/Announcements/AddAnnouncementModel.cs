using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class AddAnnouncementModel : IViewModel
    {
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        [Required]
        public AnnouncementEstateType AnnouncementEstateType { get; set; }
        public AnnouncementResidentialType? AnnouncementResidentialType { get; set; }
        public List<AnnouncementFeaturesType> Features { get; set; }
        public string Address { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int? BedroomCount { get; set; }
        public int? BathroomCount { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TitleArabian { get; set; }
        public string DescriptionArabian { get; set; }
        public bool? IsOtherPeriod { get; set; }

        public int? SittingCount { get; set; } // can be null every time
        public decimal? LivingArea { get; set; } // check
        public decimal? KitchenArea { get; set; } // check
        public decimal? BalconyArea { get; set; } // ccheck
        public decimal? LaundryArea { get; set; } // check
        //public ImageOptimizer Photo { get; set; }
        //public List<ImageOptimizer> Photos { get; set; }
        public int CurrencyId { get; set; }
        public int? Floor { get; set; }
        public CommercialType? CommercialType { get; set; }
        public ConstructionStatus? ConstructionStatus { get; set; }// can be null every time
        public SaleType? SaleType { get; set; }// can be null every time
        public FurnishingStatus? FurnishingStatus { get; set; }// can be null every time
        public OwnerShip? OwnerShip { get; set; }// can be null every time
        public BuildingAge? BuildingAge { get; set; }// can be null every time
        public LandType? LandType { get; set; }// can be null every time
        public LandCategory? LandCategory { get; set; }// can be null every time
        public FacadeType? FacadeType { get; set; }// can be null every time
        public int? LandNumber { get; set; } // can be null every time
        public int? PlanNumber { get; set; } // can be null every time
        public string DisctrictName { get; set; } // can be null every time
        public decimal? MeterPrice { get; set; } // delete
        public decimal? StreetWidth { get; set; } // can be null every time
        public int? NumberOfAppartment { get; set; }
        public int? NumberOfFloors { get; set; }
        public int? NumberOfVilla { get; set; }
        public int? NumberOfShop { get; set; }
        public int? NumberOfWareHouse { get; set; }
        public int? NumberOfUnits { get; set; } // check
        public string District { get; set; } // check
        public bool FireSystem { get; set; } // can be null every time
        public bool OfficeSpace { get; set; } // can be null every time
        public bool LaborResidence { get; set; }// can be null every time
    }
}