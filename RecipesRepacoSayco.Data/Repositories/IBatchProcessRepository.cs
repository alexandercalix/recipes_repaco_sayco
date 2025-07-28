using System;
using RecipesRepacoSayco.Data.Models;

namespace RecipesRepacoSayco.Data.Repositories;

public interface IBatchProcessRepository
{
    Task<BatchProcess> CreateAsync(BatchProcess batch);
    Task<BatchProcess?> GetByIdAsync(int id);
    Task<IEnumerable<BatchProcess>> GetByDate(DateTime start, DateTime end, int? searchBatch = null);
    Task<BatchProcess?> GetLastOpenBatchAsync();
    Task<List<BatchProcess>> GetByBatch(int batch);
    Task UpdateAsync(BatchProcess batch);
    Task DeleteAsync(int id);
}