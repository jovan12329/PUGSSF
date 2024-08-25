using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class RideForAcceptDTO
    {
        [DataMember]
        public Guid DriverId { get; set; }
        [DataMember]
        public Guid RideId { get; set; }
        public RideForAcceptDTO() { }
    }
}
