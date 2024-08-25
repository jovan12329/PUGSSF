using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class TripDTO
    {
        [DataMember]
        public string Destination { get; set; }
        [DataMember]
        public string CurrentLocation { get; set; }

        public TripDTO()
        {
        }

    }
}
