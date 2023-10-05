using HistoryService.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
            using var dbSpan = Monitoring.Monitoring.ActivitySource.StartActivity("DB Insert", ActivityKind.Internal, Activity.Current?.Context ?? default);
            try
            {
                dbSpan?.SetTag("db.table", "Calculations");
                dbSpan?.SetTag("db.operation", "INSERT");

                Monitoring.Monitoring.Log.Information("Inserting a new CalculationHistory record");
                var newCalculation = _db.Calculations.Add(entity).Entity;
                _db.SaveChanges();

                return newCalculation;
            }
            catch (Exception ex)
            {
                Monitoring.Monitoring.Log.Error(ex, "Error while inserting a new CalculationHistory record");
                throw;
            }
            finally
            {
                dbSpan?.Stop();
            }
    
        }

        public CalculationHistory Get(Guid id)
        {
            using var dbSpan = Monitoring.Monitoring.ActivitySource.StartActivity("DB Get", ActivityKind.Internal, Activity.Current?.Context ?? default);

            try
            {
                dbSpan?.SetTag("db.table", "Calculations");
                dbSpan?.SetTag("db.operation", "SELECT");
                dbSpan?.SetTag("db.record_id", id.ToString());

                Monitoring.Monitoring.Log.Information($"Fetching CalculationHistory record for ID: {id}.");

                var calculation = _db.Calculations.FirstOrDefault(c => c.Id == id);

                if (calculation == null)
                {
                    Monitoring.Monitoring.Log.Warning($"No CalculationHistory record found for ID: {id}.");
                }

                return calculation;
            }
            catch(Exception ex)
            {
                Monitoring.Monitoring.Log.Error(ex, $"Error while getting a CalculationHistory with id {id}");
                throw;
            }
            finally
            {
                dbSpan?.Stop();
            }
        
        }

        public IEnumerable<CalculationHistory> GetAll()
        {
            using var dbSpan = Monitoring.Monitoring.ActivitySource.StartActivity("DB GetAll", ActivityKind.Internal, Activity.Current?.Context ?? default);

            try
            {
                dbSpan?.SetTag("db.table", "Calculations");
                dbSpan?.SetTag("db.operation", "SELECT_ALL");

                Monitoring.Monitoring.Log.Information("Fetching all CalculationHistory records.");

                var calculations = _db.Calculations.ToList();

                return calculations;
            }
            catch (Exception ex)
            {
                Monitoring.Monitoring.Log.Error(ex, "Error while fetching all CalculationHistory records.");
                throw;
            }
            finally
            {
                dbSpan?.Stop();
            }
        }
    }
}
