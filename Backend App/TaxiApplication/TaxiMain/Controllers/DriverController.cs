using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace TaxiMain.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class DriverController : ControllerBase
    {


        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        public DriverController(IConfiguration config, Common.Interfaces.IEmailSender emailSender)
        {
            _config = config;
            this._emailSender = emailSender;
        }



        [Authorize(Policy = "Driver")]
        [HttpPut]
        public async Task<IActionResult> AcceptNewRide([FromBody] RideForAcceptDTO ride)//Driver Serv
        {
            try
            {
                var fabricClient = new FabricClient();
                RoadTrip result = null;

                
                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.AcceptRoadTripDriver(ride.RideId, ride.DriverId);
               

                if (result != null)
                {
                    var response = new
                    {
                        ride = result,
                        message = "Sucessfuly accepted driver!"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("This id does not exist");
                }

            }
            catch
            {
                return BadRequest("Something went wrong!");
            }
        }


        [Authorize(Policy = "Driver")]
        [HttpGet]
        public async Task<IActionResult> GetAllUncompletedRides()//Driving Serv
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTrip> result = null;

                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.GetRoadTrips();
                    

                

                if (result != null)
                {

                    var response = new
                    {
                        rides = result,
                        message = "Succesfuly get list of not completed rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Incorrect email or password");
                }

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while registering new User");
            }
        }

        [Authorize(Policy = "Driver")]
        [HttpGet]
        public async Task<IActionResult> GetCompletedRidesForDriver([FromQuery] Guid id)//Driving Serv
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTrip> result = null;

                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.GetListOfCompletedRidesForDriver(id);
                    

                

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





        [Authorize(Policy = "Driver")]
        [HttpGet]
        public async Task<IActionResult> GetCurrentTripDriver(Guid id)
        {
            try
            {

                var fabricClient = new FabricClient();
                RoadTrip result = null;

                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.GetCurrentTripDriver(id);
                    

                

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



    }
}
