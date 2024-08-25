using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Collections.Generic;
using System.Fabric;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaxiMain.Tools;

namespace TaxiMain.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {


        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        public TenantsController(IConfiguration config, Common.Interfaces.IEmailSender emailSender)
        {
            _config = config;
            this._emailSender = emailSender;
        }


        [HttpPost]
        public async Task<IActionResult> RegularRegister([FromForm] TenantRegisterDTO userData)
        {
            if (string.IsNullOrEmpty(userData.Email) || !EmailChecker.IsValidEmail(userData.Email)) return BadRequest("Invalid email format");
            if (string.IsNullOrEmpty(userData.Password)) return BadRequest("Password cannot be null or empty");
            if (string.IsNullOrEmpty(userData.Username)) return BadRequest("Username cannot be null or empty");
            if (string.IsNullOrEmpty(userData.FirstName)) return BadRequest("First name cannot be null or empty");
            if (string.IsNullOrEmpty(userData.LastName)) return BadRequest("Last name cannot be null or empty");
            if (string.IsNullOrEmpty(userData.Address)) return BadRequest("Address cannot be null or empty");
            if (string.IsNullOrEmpty(userData.TypeOfUser)) return BadRequest("Type of user must be selected!");
            if (string.IsNullOrEmpty(userData.Birthday)) return BadRequest("Birthday need to be selected!");
            if (userData.ImageUrl.Length == 0) return BadRequest("You need to send image while doing registration!");
            try
            {

                Tenant tenantForRegister = new Tenant(userData);

                var fabricClient = new FabricClient();
                bool result = false;

                
                    
                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result = await proxy.AddNewUser(tenantForRegister);
                

                if (result) return Ok($"Successfully registered new User: {userData.Username}");
                else return StatusCode(409, "User already exists in database!");


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while registering new User");
            }
        }



        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginTenantDTO user)
        {
            if (string.IsNullOrEmpty(user.Email) || !EmailChecker.IsValidEmail(user.Email)) return BadRequest("Invalid email format");
            if (string.IsNullOrEmpty(user.Password)) return BadRequest("Password cannot be null or empty");

            try
            {
                var fabricClient = new FabricClient();
                LogedTenantDTO result = null; 


                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result= await proxy.LoginUser(user);

                   

                if (result != null)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim("MyCustomClaim", result.Roles.ToString()));

                    var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
                        _config["Jwt:Issuer"],
                        claims,
                        expires: DateTime.Now.AddMinutes(360),
                        signingCredentials: credentials);

                    var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                    var response = new
                    {
                        token = token,
                        user = result,
                        message = "Login successful"
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
                return StatusCode(500, "An error occurred while login User");
            }
        }


        [HttpGet]
        public async Task<List<TenantDTO>> GetUsers()
        {

            try
            {
                var fabricClient = new FabricClient();
                var result = new List<TenantDTO>();



                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                var partitionResult = await proxy.ListUsers();
                result.AddRange(partitionResult);
                

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<TenantDTO>(); // Return an empty list or handle the error as needed
            }
        }

        [AllowAnonymous]
        [HttpPut]
        public async Task<IActionResult> ChangeUserFields([FromForm] TenantUpdateDTO user)
        {
            UserForUpdateOverNetwork userForUpdate = new UserForUpdateOverNetwork(user);

            try
            {
                var fabricClient = new FabricClient();
                TenantDTO result = null;

                
                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result = await proxy.ChangeUserFields(userForUpdate);

                if (result != null)
                {
                    var response = new
                    {
                        changedUser = result,
                        message = "Succesfuly changed user fields!"
                    };
                    return Ok(response);
                }
                else return StatusCode(409, "User for change is not in db!");

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating user");
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetUserInfo([FromQuery] Guid id)
        {
            try
            {
                var fabricClient = new FabricClient();
                TenantDTO result = null;

                var proxy = ServiceProxy.Create<ITenantsService>(new Uri("fabric:/TaxiApplication/TenantsService"), new ServicePartitionKey(0));
                result = await proxy.GetUserInfo(id);
                    

                if (result != null)
                {
                    var response = new
                    {
                        user = result,
                        message = "Successfully retrieved user info"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("This id does not exist");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving user info");
            }
        }












    }
}
