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
        try
        {
            _context.BatchProcesses.Add(batch);
            await _context.SaveChangesAsync();
            return batch;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving batch process: {ex.Message}");
            throw;
        }
    }

    public async Task<BatchProcess?> GetByIdAsync(int id)
    {
        var data = await _context.BatchProcesses.FindAsync(id);
        if (data is null)
        {
            Console.WriteLine($"⚠️ Batch process with ID {id} not found.");
            return null;
        }
        Console.WriteLine($"✅ Batch process with ID {id} retrieved successfully.");
        return data;
    }

    public async Task<BatchProcess?> GetLastOpenBatchAsync()
    {
        return await _context.BatchProcesses
       .Where(b => b.EndTime == null)
       .OrderByDescending(b => b.StartTime)
       .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<BatchProcess>> GetByDate(DateTime start, DateTime end, int? searchBatch = null)
    {
        var query = _context.BatchProcesses
            .Where(x => x.StartTime >= start && x.EndTime <= end);

        if (searchBatch.HasValue)
            query = query.Where(x => x.Batch == searchBatch.Value);

        return await query.OrderByDescending(x => x.StartTime).ToListAsync();
    }



    public async Task<List<BatchProcess>> GetByBatch(int batch)
    {
        return await _context.BatchProcesses
            .Where(p => p.Batch == batch)
            .OrderBy(p => p.StartTime)
            .ToListAsync();
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
