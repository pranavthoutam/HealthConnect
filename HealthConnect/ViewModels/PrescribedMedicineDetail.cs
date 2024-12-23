namespace HealthConnect.ViewModels
{
    public class PrescribedMedicineDetail
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public List<string> Timings { get; set; }
        [Required]
        public string FoodInstruction { get; set; }
        public string? AdditionalAdvice { get; set; }
    }
}
