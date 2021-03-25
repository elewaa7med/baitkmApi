using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class NearbyProcedureResponse : IStoredProcedureResponse
    {
        public int Id { get; set; }
        public int Distance { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}
