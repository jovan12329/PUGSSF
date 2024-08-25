using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class ReviewDTO
    {
        [DataMember]
        public Guid tripId { get; set; }
        [DataMember]
        public int rating { get; set; }

        public ReviewDTO()
        {
        }



    }
}
