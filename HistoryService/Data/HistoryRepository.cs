using HistoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace HistoryService.Data
{
    public class HistoryRepository : IRepository<CalculationHistory>
    {
        private readonly HistoryContext _db;

        public HistoryRepository(HistoryContext context)
        {
            _db = context;
        }
        public CalculationHistory Add(CalculationHistory entity)
        {
            var newCalculation = _db.Calculations.Add(entity).Entity;
            _db.SaveChanges();
            return newCalculation;
        }

        public CalculationHistory Get(Guid id)
        {
            return _db.Calculations.FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<CalculationHistory> GetAll()
        {
            return _db.Calculations.ToList();
        }
    }
}
