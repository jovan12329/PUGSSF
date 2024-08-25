﻿using Common.Enumerations;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    [DataContract]
    public class TenantDTO
    {


        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public double AverageRating { get; set; }

        [DataMember]
        public int SumOfRatings { get; set; }

        [DataMember]
        public int NumOfRatings { get; set; }

        [DataMember]
        public DateTime Birthday { get; set; }

        [DataMember]
        public string Email { get; set; }


        [DataMember]
        public bool IsVerified { get; set; }

        [DataMember]
        public bool IsBlocked { get; set; }


        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public PolicyRoles Roles { get; set; }

        [DataMember]
        public BlobFile ImageFile { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public Common.Enumerations.Status Status { get; set; }

        [DataMember]
        public Guid Id { get; set; }


        public TenantDTO(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string username, PolicyRoles roles, BlobFile imageFile, string password, Common.Enumerations.Status status, Guid id)
        {
            Address = address;
            AverageRating = averageRating;
            SumOfRatings = sumOfRatings;
            NumOfRatings = numOfRatings;
            Birthday = birthday;
            Email = email;
            IsVerified = isVerified;
            IsBlocked = isBlocked;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Roles = roles;
            ImageFile = imageFile;
            Password = password;
            Status = status;
            Id = id;
        }

        public TenantDTO()
        {
        }





    }
}
