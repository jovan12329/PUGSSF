using Common.DTOs;
using Common.Enumerations;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [DataContract]
    public class Tenant
    {
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Username { get; set; }
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
        public PolicyRoles TypeOfUser { get; set; }
        [DataMember]
        public BlobFile ImageFile { get; set; }
        [DataMember]
        public Status Status { get; set; }

        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string ImageUrl { get; set; }


        public Tenant(TenantRegisterDTO userRegister)
        {
            FirstName = userRegister.FirstName;
            LastName = userRegister.LastName;
            Birthday = DateTime.ParseExact(userRegister.Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            Address = userRegister.Address;
            Email = userRegister.Email;
            Password = userRegister.Password;
            TypeOfUser = Enum.TryParse<PolicyRoles>(userRegister.TypeOfUser, true, out var role) ? role : PolicyRoles.Rider;
            Username = userRegister.Username;
            Id = Guid.NewGuid();
            switch (TypeOfUser)
            {
                case PolicyRoles.Admin:
                    IsVerified = true;
                    break;
                case PolicyRoles.Rider:
                    IsVerified = true;
                    break;
                case PolicyRoles.Driver:
                    AverageRating = 0.0;
                    IsVerified = false;
                    NumOfRatings = 0;
                    SumOfRatings = 0;
                    IsBlocked = false;
                    Status = Enumerations.Status.Procesira;
                    break;

            }
            ImageFile = ImageStreamConverter.ImgStreamConverter(userRegister.ImageUrl);
        }



        public Tenant()
        {
        }

        public Tenant(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string password, string username, PolicyRoles typeOfUser, BlobFile imageFile, Guid id)
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
            Password = password;
            Username = username;
            TypeOfUser = typeOfUser;
            ImageFile = imageFile;
            Id = id;
        }

        public Tenant(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string password, string username, PolicyRoles typeOfUser, BlobFile imageFile, string imageUrl, Status s, Guid id) : this(address, averageRating, sumOfRatings, numOfRatings, birthday, email, isVerified, isBlocked, firstName, lastName, password, username, typeOfUser, imageFile, id)
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
            Password = password;
            Username = username;
            TypeOfUser = typeOfUser;
            ImageFile = imageFile;
            ImageUrl = imageUrl;
            Status = s;
            Id = id;
        }




    }
}
