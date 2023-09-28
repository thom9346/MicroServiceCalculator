using HistoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace HistoryService.Data
{
    public class HistoryContext : DbContext
    {
        public DbSet<CalculationHistory> Calculations { get; set; }

        public HistoryContext(DbContextOptions<HistoryContext> options) : base(options) { }
    }
}
