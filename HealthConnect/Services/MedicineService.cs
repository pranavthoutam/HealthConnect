
using Microsoft.Extensions.Caching.Memory;

namespace HealthConnect.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IMemoryCache _cache;

        public MedicineService(IMedicineRepository medicineRepository,IMemoryCache cache)
        {
            _medicineRepository = medicineRepository;
            _cache = cache;
        }

        public IEnumerable<string> GetSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<string>();

            string cacheKey = $"MedicineSuggestions_{query.ToLower()}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<string> suggestions))
            {
                suggestions = _medicineRepository.GetMedicinesByPartialName(query).Take(4).ToList();

                // Cache the suggestions for 10 minutes
                _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(10));
            }

            return suggestions;
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
