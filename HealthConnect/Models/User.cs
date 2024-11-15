using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthConnect.Models
{
    public class User : IdentityUser
    {

        public Gender Gender { get; set; }

        public BloodGroup BloodGroup { get; set; }

        [DataType(DataType.Date)]
        public string DateofBirth { get; set; }
        public byte[] ProfilePhoto { get; set; }

        [ForeignKey("Address")]
        public int Address { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Others
    }

    public enum BloodGroup
    {
        OPositive,
        ONegative,
        APositive,
        ANegative,
        BPositive,
        BNegative,
        ABPositive,
        ABNegative,
    }

    public class Address
    {
        public int Id { get; set; }

        public string HouseNumber { get; set; }

        public string Street { get; set; }
        public string City { get; set; }

        public string PostalCode { get; set; }

        public string State { get; set; }

        public string Country { get; set; }
    }
}

