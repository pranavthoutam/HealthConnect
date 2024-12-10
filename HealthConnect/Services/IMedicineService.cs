namespace HealthConnect.Services
{
    public interface IMedicineService
    {
        IEnumerable<string> GetSuggestions(string query);
        List<Medicine> GetMedicinesByCategory(int categoryId);
        Medicine GetMedicineById(int id);
        Medicine? GetMedicineByName(string name);
        List<Medicine> GetMedicineByNameAndCategory(string name, int categoryId);
        byte[]? GetMedicineImage(int id);
    }
}
