using System.ComponentModel.DataAnnotations;
using HealthConnect.ViewModels.Validations;

namespace HealthConnect.ViewModels
{
    public class DoctorRegistrationViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        public string MciRegistrationNumber { get; set; }

        [Required]
        public int Experience { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal ConsultationFee { get; set; }

        public bool OnlineConsultation { get; set; }

        [AtLeastOneRequired]
        public bool ClinicAppoinment { get; set; }

        public string? ClinicLocation { get; set; }

        public string ClinicName { get; set; }

        public string HnoAndStreetName { get; set; }

        public string District { get; set; }

        public string Place { get; set; }

        public List<string> Places { get; set; } = new List<string>
{
    "Madhapur", "Gachibowli", "Uppal", "LB Nagar", "Kukatpally"
};

        [Display(Name = "Upload Clinic Image")]
        public IFormFile ClinicImage { get; set; }

        [Required]
        [Display(Name = "Upload Certificate")]
        public IFormFile Certificate { get; set; }
    }
}
