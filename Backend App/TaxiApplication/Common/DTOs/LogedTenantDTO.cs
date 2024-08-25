using Common.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class LogedTenantDTO
    {


        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public PolicyRoles Roles { get; set; }

        public LogedTenantDTO(Guid id, PolicyRoles roles)
        {
            Id = id;
            Roles = roles;
        }

        public LogedTenantDTO()
        {
        }



    }
}
