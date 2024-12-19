namespace HealthConnect.ViewModels
{
    public class PrescribedMedicineDetail
    {
        public string Name { get; set; }
        public List<string> Timings { get; set; }
        public string FoodInstruction { get; set; }
        public string AdditionalAdvice { get; set; }
    }
}
