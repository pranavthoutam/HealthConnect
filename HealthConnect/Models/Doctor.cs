using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthConnect.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId    { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal ConsultationFee { get; set; }    

        public float Rating { get; set; }

        public string ApprovalStatus { get; set; }

        [Required]
        public string MciRegistrationNumber { get; set; }

        public string FullName { get; set; }

        [Required]
        public int Experience  { get; set; }

        [Required]
        public bool OnlineConsultation { get; set; }

        [Required]
        public bool ClinicAppoinment { get; set; }

        public string ClinicName    { get; set; }
        public byte[] ClinicImage { get; set; }

        public byte[] Certificate { get; set; } 

    }
}
