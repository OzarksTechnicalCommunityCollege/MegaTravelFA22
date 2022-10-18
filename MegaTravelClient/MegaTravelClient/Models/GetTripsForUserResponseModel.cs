

namespace MegaTravelClient.Models
{
    public class GetTripsForUserResponseModel
    {
        public bool Status { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        // not included in answer set
        //public UserData userData { get; set; }
        public List<TripData> tripList { get; set; }
    }
}
