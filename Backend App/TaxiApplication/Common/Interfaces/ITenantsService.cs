using Common.DTOs;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITenantsService:IService
    {


        
        Task<bool> AddNewUser(Tenant user);

        
        Task<LogedTenantDTO> LoginUser(LoginTenantDTO loginUserDTO);

        
        Task<List<TenantDTO>> ListUsers();

        
        Task<List<DriverViewDTO>> ListDrivers();

        
        Task<bool> ChangeDriverStatus(Guid id, bool status);

        
        Task<TenantDTO> ChangeUserFields(UserForUpdateOverNetwork user);

        
        Task<TenantDTO> GetUserInfo(Guid id);

        
        Task<bool> VerifyDriver(Guid id, string email, string action);

        
        Task<List<DriverViewDTO>> GetNotVerifiedDrivers();


    }
}
