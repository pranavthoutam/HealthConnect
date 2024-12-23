namespace HealthConnect.ViewModels
{
    public class PrescriptionViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; }
        public List<PrescribedMedicineDetail> MedicineList { get; set; }

        public PrescriptionViewModel()
        {
            // Initialize the MedicineList to avoid null reference
            MedicineList = new List<PrescribedMedicineDetail>();
        }
    }
}
