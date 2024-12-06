namespace HealthConnect.Models
{
    public class User : IdentityUser
    {

        public Gender? Gender { get; set; }

        public BloodGroup? BloodGroup { get; set; }

        [DataType(DataType.Date)]
        public string? DateofBirth { get; set; }
        public byte[]? ProfilePhoto { get; set; }

        public string? HouseNumber { get; set; }

        public string? Street { get; set; }
        public string? City { get; set; }

        [StringLength(6)]

        public string? PostalCode { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }
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
}

