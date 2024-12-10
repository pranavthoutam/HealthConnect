public class MedicineRepository : IMedicineRepository
{
    private readonly ApplicationDbContext _context;
    public MedicineRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Medicine> GetMedicinesByCategory(int categoryId)
    {
        return _context.Medicines
            .Where(m => m.CategoryId == categoryId)
            .ToList();
    }

    public Medicine GetMedicineById(int id)
    {
        return _context.Medicines
            .Include(m => m.Alternatives)
            .ThenInclude(ma => ma.Alternative)
            .FirstOrDefault(m => m.Id == id);
    }

    public IEnumerable<string> GetMedicinesByPartialName(string partialName)
    {
        return _context.Medicines
            .Where(m => EF.Functions.Like(m.Name, $"%{partialName}%"))
            .Select(m => m.Name)
            .ToList();
    }


    public List<Medicine> GetMedicineByName(string name, int? categoryId = null)
    {
        return _context.Medicines
            .Include(m => m.Alternatives)
             .ThenInclude(ma => ma.Alternative)
            .Where(m => m.Name.Contains(name) &&
                        (!categoryId.HasValue || m.CategoryId == categoryId.Value))
            .ToList();
    }
}