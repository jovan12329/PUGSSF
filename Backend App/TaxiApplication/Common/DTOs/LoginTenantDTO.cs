using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class LoginTenantDTO
    {

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        public LoginTenantDTO(string email, string password)
        {
            Email = email;
            Password = password;
        }


    }
}
