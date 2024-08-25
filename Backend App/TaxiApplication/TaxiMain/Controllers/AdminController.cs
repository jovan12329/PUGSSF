using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Collections.Generic;
using System.Fabric;

namespace TaxiMain.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        public AdminController(IConfiguration config, Common.Interfaces.IEmailSender emailSender)
        {
            _config = config;
            this._emailSender = emailSender;
        }


        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            try
            {

                var fabricClient = new FabricClient();
                List<DriverViewDTO> result = null;

                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result = await proxy.ListDrivers();
                    
                if (result != null)
                {

                    var response = new
                    {
                        drivers = result,
                        message = "Succesfuly get list of drivers"
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


        [Authorize(Policy = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeDriverStatus([FromBody] DriverChangeStatusDTO driver)
        {
            try
            {

                var fabricClient = new FabricClient();
                bool result = false;

                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result = await proxy.ChangeDriverStatus(driver.Id, driver.Status);
                
               

                if (result) return Ok("Succesfuly changed driver status");

                else return BadRequest("Driver status is not changed");

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while registering new User");
            }
        }


        [Authorize(Policy = "Admin")]
        [HttpPut]
        public async Task<IActionResult> VerifyDriver([FromBody] DriverVerificationDTO driver)
        {
            try
            {
                var fabricClient = new FabricClient();
                bool result = false;

                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result = await proxy.VerifyDriver(driver.Id, driver.Email, driver.Action);
                    

                if (result)
                {
                    var response = new
                    {
                        Verified = result,
                        message = $"Driver with id:{driver.Id} is now changed status of verification to:{driver.Action}"
                    };
                    if (driver.Action == "Prihvacen") await _emailSender.SendEmailAsync(driver.Email, "Account verification", "You have been successfuly verified on taxi app, now you can drive!");

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


        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetDriversForVerification()
        {
            try
            {

                var fabricClient = new FabricClient();
                List<DriverViewDTO> result = null;

                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result = await proxy.GetNotVerifiedDrivers();
                    

                if (result != null)
                {

                    var response = new
                    {
                        drivers = result,
                        message = "Succesfuly get list of drivers"
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


        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetCompletedRidesAdmin()//Driving Serv
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTrip> result = null;

                 
                var proxy = ServiceProxy.Create<IDrive>(new Uri("fabric:/TaxiApplication/DrivingService"), new ServicePartitionKey(0));
                result = await proxy.GetListOfCompletedRidesAdmin();
                    

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




    }
}
