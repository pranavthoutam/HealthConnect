
namespace HealthConnect.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [ForeignKey("MedicineCategory")]
        public int CategoryId { get; set; }

        public string Manufacturer { get; set; }

        public string SideEffects { get; set; }

        public string Usage { get; set; }

        public string Precautions { get; set; }

        public string Dosage { get; set; }

        public byte[]? Image { get; set; }

        // Navigation property for MedicineCategory
        public virtual MedicineCategory MedicineCategory { get; set; }

        // Navigation property for alternatives
        public virtual ICollection<MedicineAlternatives> Alternatives { get; set; }=new List<MedicineAlternatives>();
    }
}
