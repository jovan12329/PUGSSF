using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class TenantRegisterDTO
    {

        public TenantRegisterDTO()
        {
        }

        [DataMember]
        public string? FirstName { get; set; }
        [DataMember]
        public string? LastName { get; set; }
        [DataMember]
        public string? Birthday { get; set; }
        [DataMember]
        public string? Address { get; set; }
        [DataMember]
        public string? Email { get; set; }
        [DataMember]
        public string? Password { get; set; }
        [DataMember]
        public IFormFile? ImageUrl { get; set; }
        [DataMember]
        public string? TypeOfUser { get; set; }
        [DataMember]
        public string? Username { get; set; }


    }
}
