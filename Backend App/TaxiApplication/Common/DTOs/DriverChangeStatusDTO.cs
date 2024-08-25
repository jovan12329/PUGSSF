using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class DriverChangeStatusDTO
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public bool Status { get; set; }
        
        public DriverChangeStatusDTO() { }


    }
}
