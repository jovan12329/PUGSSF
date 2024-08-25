using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IDrive:IService
    {

        
        Task<RoadTrip> AcceptRoadTrip(RoadTrip trip);

        Task<RoadTrip> GetCurrentRoadTrip(Guid id);

        Task<List<RoadTrip>> GetRoadTrips();

        Task<RoadTrip> AcceptRoadTripDriver(Guid rideId, Guid driverId);

        Task<List<RoadTrip>> GetListOfCompletedRidesForDriver(Guid driverId);

        Task<List<RoadTrip>> GetListOfCompletedRidesForRider(Guid driverId);

        Task<List<RoadTrip>> GetListOfCompletedRidesAdmin();

        Task<RoadTrip> GetCurrentTrip(Guid id);

        Task<RoadTrip> GetCurrentTripDriver(Guid id);

        Task<List<RoadTrip>> GetAllNotRatedTrips();

        Task<bool> SubmitRating(Guid tripId, int rating);


    }
}
