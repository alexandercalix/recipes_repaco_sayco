using System;
using Microsoft.EntityFrameworkCore;
using RecipesRepacoSayco.Data.Data;
using RecipesRepacoSayco.Data.Models;

namespace RecipesRepacoSayco.Data.Repositories;

public class BatchProcessRepository : IBatchProcessRepository
{
    private readonly ApplicationDbContext _context;

    public BatchProcessRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BatchProcess> CreateAsync(BatchProcess batch)
    {
        _context.BatchProcesses.Add(batch);
        await _context.SaveChangesAsync();
        return batch;
    }

    public async Task<BatchProcess?> GetByIdAsync(int id)
    {
        return await _context.BatchProcesses.FindAsync(id);
    }

    public async Task<List<BatchProcess>> GetByDateAndTextAsync(DateTime start, DateTime end, string? name)
    {
        var query = _context.BatchProcesses
            .Where(b => b.StartTime >= start && b.EndTime <= end);

        if (!string.IsNullOrWhiteSpace(name))
        {
            string lowered = name.ToLower();
            query = query.Where(b => b.ProductName.ToLower().Contains(lowered));
        }

        return await query.ToListAsync();
    }

    public async Task UpdateAsync(BatchProcess batch)
    {
        _context.BatchProcesses.Update(batch);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var batch = await _context.BatchProcesses.FindAsync(id);
        if (batch is not null)
        {
            _context.BatchProcesses.Remove(batch);
            await _context.SaveChangesAsync();
        }
    }
}
