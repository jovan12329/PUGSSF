using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace TaxiMain.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class RiderController : ControllerBase
    {


        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        public RiderController(IConfiguration config, Common.Interfaces.IEmailSender emailSender)
        {
            _config = config;
            this._emailSender = emailSender;
        }



        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetEstimatedPrice([FromQuery] TripDTO trip)
        {
            double rangeMin = 7.3;
            double rangeMax = 24.5;

            Random r = new Random();
            double price = rangeMin + (rangeMax - rangeMin) * r.NextDouble();

            
            TimeSpan estimatedTimeMin = new TimeSpan(0, 1, 0); 
            TimeSpan estimatedTimeMax = new TimeSpan(0, 2, 0); 

            Estimation estimation = new Estimation(price,estimatedTimeMin,estimatedTimeMax);
            if (estimation != null)
            {

                var response = new
                {
                    price = estimation,
                    message = "Succesfuly get estimation"
                };
                return Ok(response);
            }
            else
            {
                return StatusCode(500, "An error occurred while estimating price");
            }

        }


        [Authorize(Policy = "Rider")]
        [HttpPut]
        public async Task<IActionResult> AcceptSuggestedDrive([FromBody] AcceptedRoadTripDTO acceptedRoadTrip)//Driving Serv
        {
            try
            {
                if (string.IsNullOrEmpty(acceptedRoadTrip.Destination)) return BadRequest("You must send destination!");
                if (string.IsNullOrEmpty(acceptedRoadTrip.CurrentLocation)) return BadRequest("You must send location!");
                if (acceptedRoadTrip.Accepted == true) return BadRequest("Ride cannot be automaticaly accepted!");
                if (acceptedRoadTrip.Price == 0.0 || acceptedRoadTrip.Price < 0.0) return BadRequest("Invalid price!");


                var fabricClient = new FabricClient();
                RoadTrip result = null;
                RoadTrip tripFromRider = new RoadTrip(acceptedRoadTrip.CurrentLocation, acceptedRoadTrip.Destination, acceptedRoadTrip.RiderId, acceptedRoadTrip.Price, acceptedRoadTrip.Accepted, acceptedRoadTrip.MinutesToDriverArrive);
                
                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.AcceptRoadTrip(tripFromRider);
                    
                

                if (result != null)
                {
                    var response = new
                    {
                        Drive = result,
                        message = "Successfully scheduled"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("You already submited ticked!");
                }


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while accepting new drive!");
            }
        }


        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetCompletedRidesForRider([FromQuery] Guid id)//Driving Serv
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTrip> result = null;

                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.GetListOfCompletedRidesForRider(id);
                    

                

                if (result != null)
                {

                    var response = new
                    {
                        rides = result,
                        message = "Succesfuly get list completed rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Incorrect email or password");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetCurrentTrip(Guid id)//Driving Serv
        {
            try
            {

                var fabricClient = new FabricClient();
                RoadTrip result = null;

                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.GetCurrentTrip(id);
                    

                if (result != null)
                {

                    var response = new
                    {
                        trip = result,
                        message = "Succesfuly get current ride"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetAllNotRatedTrips()
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTrip> result = null;

                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.GetAllNotRatedTrips();
                    

                if (result != null)
                {

                    var response = new
                    {
                        rides = result,
                        message = "Succesfuly get unrated rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [Authorize(Policy = "Rider")]
        [HttpPut]
        public async Task<IActionResult> SubmitRating([FromBody] ReviewDTO review)
        {
            try
            {

                var fabricClient = new FabricClient();
                bool result = false;

                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.SubmitRating(review.tripId, review.rating);
                    

                

                if (result != false)
                {
                    return Ok("Sucessfuly submited rating");
                }
                else
                {
                    return BadRequest("Rating is not submited");
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }



    }
}
