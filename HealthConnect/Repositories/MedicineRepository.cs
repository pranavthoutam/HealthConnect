using HealthConnect.Data;
using HealthConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthConnect.Repositories
{
    public class MedicineRepository
    {
       private readonly ApplicationDbContext _context;
        public MedicineRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<Medicine> getMedicines(int categoryid)
        {
            List<Medicine> medicines = new List<Medicine>();
            medicines = _context.Medicines.Where(m => m.CategoryId==categoryid).ToList();
            return medicines;
        }
        public Medicine GetMedicineById(int id)
        {
            return _context.Medicines
                           .Include(m => m.Alternatives)
                               .ThenInclude(ma => ma.Alternative) // Load the alternative medicines
                           .FirstOrDefault(m => m.Id == id);
        }

        public IEnumerable<string> GetMedicinesByPartialName(string partialName)
        {
            return _context.Medicines
                           .Where(m => m.Name.Contains(partialName))
                           .Select(m => m.Name)
                           .ToList();
        }

        public List<Medicine> GetMedicineByName(string? name,int? categoryId)
        {
            return _context.Medicines
                .Where(m=> m.Name.Contains(name) && m.CategoryId==categoryId)
                .ToList();
        }

        public IEnumerable<Medicine> GetMedicineByName(string name)
        {
            return _context.Medicines.
                Where(m => m.Name.Contains(name));
        }


}
}
