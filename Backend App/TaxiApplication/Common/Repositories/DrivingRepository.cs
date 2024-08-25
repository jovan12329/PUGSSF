using Common.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class DrivingRepository
    {

        private CloudStorageAccount cloudAcc;
        private CloudTableClient tableClient;
        private CloudTable _trips;


        public CloudStorageAccount CloudAcc { get => cloudAcc; set => cloudAcc = value; }
        public CloudTableClient TableClient { get => tableClient; set => tableClient = value; }
        public CloudTable Trips { get => _trips; set => _trips = value; }


        public DrivingRepository(string tableName)
        {
            

                CloudAcc = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("DataConnectionString")); 

                TableClient = CloudAcc.CreateCloudTableClient(); 

                Trips = TableClient.GetTableReference(tableName); 

                Trips.CreateIfNotExistsAsync().Wait();

            
        }

        

        public IEnumerable<RoadTripEntity> GetAllTrips()
        {
            var q = new TableQuery<RoadTripEntity>();
            var qRes = Trips.ExecuteQuerySegmentedAsync(q, null).GetAwaiter().GetResult();
            return qRes.Results;
        }

        public async Task<bool> UpdateEntity(Guid driverId, Guid rideId)
        {
            TableQuery<RoadTripEntity> rideQuery = new TableQuery<RoadTripEntity>()
            .Where(TableQuery.GenerateFilterConditionForGuid("TripId", QueryComparisons.Equal, rideId));

            TableQuerySegment<RoadTripEntity> queryResult = await Trips.ExecuteQuerySegmentedAsync(rideQuery, null);

            if (queryResult.Results.Count > 0)
            {
                RoadTripEntity trip = queryResult.Results[0];
                trip.Accepted = true;
                trip.SecondsToEndTrip = 60;
                trip.DriverId = driverId;
                var operation = TableOperation.Replace(trip);
                await Trips.ExecuteAsync(operation);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task RateTrip(Guid tripId)
        {
            TableQuery<RoadTripEntity> rideQuery = new TableQuery<RoadTripEntity>()
            .Where(TableQuery.GenerateFilterConditionForGuid("TripId", QueryComparisons.Equal, tripId));
            TableQuerySegment<RoadTripEntity> queryResult = await Trips.ExecuteQuerySegmentedAsync(rideQuery, null);

            if (queryResult.Results.Count > 0)
            {
                RoadTripEntity trip = queryResult.Results[0];
                trip.IsRated = true;
                var operation = TableOperation.Replace(trip);
                await Trips.ExecuteAsync(operation);
            }

        }


        public async Task<bool> FinishTrip(Guid tripId)
        {
            TableQuery<RoadTripEntity> rideQuery = new TableQuery<RoadTripEntity>()
            .Where(TableQuery.GenerateFilterConditionForGuid("TripId", QueryComparisons.Equal, tripId));
            TableQuerySegment<RoadTripEntity> queryResult = await Trips.ExecuteQuerySegmentedAsync(rideQuery, null);

            if (queryResult.Results.Count > 0)
            {
                RoadTripEntity trip = queryResult.Results[0];
                trip.IsFinished = true;
                var operation = TableOperation.Replace(trip);
                await Trips.ExecuteAsync(operation);
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
