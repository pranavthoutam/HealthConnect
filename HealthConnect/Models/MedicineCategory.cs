namespace HealthConnect.Models
{
    public class MedicineCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Medicine> Medicines { get; set; }
    }
}
