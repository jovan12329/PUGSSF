using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class AcceptedRoadTripDTO
    {
        [DataMember]
        public string Destination { get; set; }  
        [DataMember]
        public string CurrentLocation { get; set; } 
        [DataMember]
        public Guid RiderId { get; set; }  
        [DataMember]
        public double Price { get; set; } 
        [DataMember]
        public bool Accepted { get; set; } 

        public int MinutesToDriverArrive { get; set; }
        public AcceptedRoadTripDTO() { }

    }
}
