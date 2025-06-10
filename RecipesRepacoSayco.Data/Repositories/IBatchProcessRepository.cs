using System;
using RecipesRepacoSayco.Data.Models;

namespace RecipesRepacoSayco.Data.Repositories;

public interface IBatchProcessRepository
{
    Task<BatchProcess> CreateAsync(BatchProcess batch);
    Task<BatchProcess?> GetByIdAsync(int id);
    Task<List<BatchProcess>> GetByDateAndTextAsync(DateTime start, DateTime end, string? name);

    Task UpdateAsync(BatchProcess batch);
    Task DeleteAsync(int id);
}