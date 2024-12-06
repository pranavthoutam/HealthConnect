namespace HealthConnect.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Medicine name is required.")]
        [StringLength(100, ErrorMessage = "Medicine name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [ForeignKey("MedicineCategory")]
        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Manufacturer is required.")]
        [StringLength(100, ErrorMessage = "Manufacturer name cannot exceed 100 characters.")]
        public string Manufacturer { get; set; }

        [StringLength(500, ErrorMessage = "Side effects description cannot exceed 500 characters.")]
        public string SideEffects { get; set; }

        [StringLength(500, ErrorMessage = "Usage instructions cannot exceed 500 characters.")]
        public string Usage { get; set; }

        [StringLength(500, ErrorMessage = "Precautions cannot exceed 500 characters.")]
        public string Precautions { get; set; }

        [StringLength(100, ErrorMessage = "Dosage information cannot exceed 100 characters.")]
        public string Dosage { get; set; }

        public byte[]? Image { get; set; }

        // Navigation property for MedicineCategory
        public virtual MedicineCategory MedicineCategory { get; set; }

        // Navigation property for alternatives
        public virtual ICollection<MedicineAlternatives> Alternatives { get; set; } = new List<MedicineAlternatives>();
    }
}
