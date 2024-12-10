
namespace HealthConnect.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _medicineRepository;

        public MedicineService(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public IEnumerable<string> GetSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<string>();

            return _medicineRepository.GetMedicinesByPartialName(query);
        }

        public List<Medicine> GetMedicinesByCategory(int categoryId)
        {
            return _medicineRepository.GetMedicinesByCategory(categoryId);
        }

        public Medicine GetMedicineById(int id)
        {
            return _medicineRepository.GetMedicineById(id);
        }

        public Medicine? GetMedicineByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var decodedName = name.Replace('-', ' ');
            return _medicineRepository.GetMedicineByName(decodedName).FirstOrDefault();
        }

        public List<Medicine> GetMedicineByNameAndCategory(string name, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<Medicine>();

            return _medicineRepository.GetMedicineByName(name, categoryId);
        }

        public byte[]? GetMedicineImage(int id)
        {
            var medicine = _medicineRepository.GetMedicineById(id);
            return medicine?.Image;
        }
    }
}
