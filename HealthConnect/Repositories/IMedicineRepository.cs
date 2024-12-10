namespace HealthConnect.Repositories
{
    public interface IMedicineRepository
    {
        List<Medicine> GetMedicinesByCategory(int categoryId);
        Medicine GetMedicineById(int id);
        IEnumerable<string> GetMedicinesByPartialName(string partialName);
        List<Medicine> GetMedicineByName(string name, int? categoryId = null);
    }
}
