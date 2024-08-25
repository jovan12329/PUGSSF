using Common.DTOs;
using Common.Entities;
using Common.Enumerations;
using Common.Helpers;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mappers
{
    public class TenantMapper
    {

        public static Tenant MapUserEntityToUser(TenantEntity u, byte[] imageOfUser)
        {
            var statusString = u.Status; 
            Status myStatus;

            if (Enum.TryParse(statusString, out myStatus))
            {
                
                return new Tenant(
                    u.Address,
                    u.AverageRating,
                    u.SumOfRatings,
                    u.NumOfRatings,
                    u.Birthday,
                    u.Email,
                    u.IsVerified,
                    u.IsBlocked,
                    u.FirstName,
                    u.LastName,
                    u.Password,
                    u.Username,
                    (PolicyRoles)Enum.Parse(typeof(PolicyRoles), u.PartitionKey),
                    new BlobFile(imageOfUser),
                    u.ImageUrl,
                    myStatus,
                    u.Id
                );
            }
            return null;
        }

        public static TenantDTO MapTenantToTenantDto(Tenant u)
        {

            return new TenantDTO(u.Address, u.AverageRating, u.SumOfRatings, u.NumOfRatings, u.Birthday, u.Email, u.IsVerified, u.IsBlocked, u.FirstName, u.LastName, u.Username, u.TypeOfUser, u.ImageFile, u.Password, u.Status, u.Id);
        }

    }
}
