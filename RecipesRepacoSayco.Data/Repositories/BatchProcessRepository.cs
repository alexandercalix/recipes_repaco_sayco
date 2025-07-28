using System;
using Microsoft.EntityFrameworkCore;
using RecipesRepacoSayco.Data.Data;
using RecipesRepacoSayco.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecipesRepacoSayco.Data.Interfaces;

namespace RecipesRepacoSayco.Data.Repositories;

public class BatchProcessRepository : IBatchProcessRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDatabaseDiagnostics _diagnostics;

    public BatchProcessRepository(ApplicationDbContext context, IDatabaseDiagnostics diagnostics)
    {
        _context = context;
        _diagnostics = diagnostics;
    }

    public async Task<BatchProcess> CreateAsync(BatchProcess batch)
    {
        try
        {
            _context.BatchProcesses.Add(batch);
            await _context.SaveChangesAsync();
            _diagnostics.LogCommand("INSERT BatchProcess", true, null, nameof(BatchProcessRepository));
            return batch;
        }
        catch (Exception ex)
        {
            _diagnostics.LogCommand("INSERT BatchProcess", false, ex.Message, nameof(BatchProcessRepository));
            throw;
        }
    }

    public async Task<BatchProcess?> GetByIdAsync(int id)
    {
        try
        {
            var data = await _context.BatchProcesses.FindAsync(id);
            if (data is null)
            {
                _diagnostics.LogCommand($"SELECT BatchProcess WHERE Id = {id}", false, "Not found", nameof(BatchProcessRepository));
                return null;
            }

            _diagnostics.LogCommand($"SELECT BatchProcess WHERE Id = {id}", true, null, nameof(BatchProcessRepository));
            return data;
        }
        catch (Exception ex)
        {
            _diagnostics.LogCommand($"SELECT BatchProcess WHERE Id = {id}", false, ex.Message, nameof(BatchProcessRepository));
            throw;
        }
    }

    public async Task<BatchProcess?> GetLastOpenBatchAsync()
    {
        try
        {
            var data = await _context.BatchProcesses
                .Where(b => b.EndTime == null)
                .OrderByDescending(b => b.StartTime)
                .FirstOrDefaultAsync();

            _diagnostics.LogCommand("SELECT Last Open Batch", true, null, nameof(BatchProcessRepository));
            return data;
        }
        catch (Exception ex)
        {
            _diagnostics.LogCommand("SELECT Last Open Batch", false, ex.Message, nameof(BatchProcessRepository));
            throw;
        }
    }

    public async Task<IEnumerable<BatchProcess>> GetByDate(DateTime start, DateTime end, int? searchBatch = null)
    {
        try
        {
            var query = _context.BatchProcesses
                .Where(x => x.StartTime >= start && x.EndTime <= end);

            if (searchBatch.HasValue)
                query = query.Where(x => x.Batch == searchBatch.Value);

            var result = await query.OrderByDescending(x => x.StartTime).ToListAsync();

            _diagnostics.LogCommand($"SELECT BatchProcess BETWEEN {start} AND {end}", true, null, nameof(BatchProcessRepository));
            return result;
        }
        catch (Exception ex)
        {
            _diagnostics.LogCommand($"SELECT BatchProcess BETWEEN {start} AND {end}", false, ex.Message, nameof(BatchProcessRepository));
            throw;
        }
    }

    public async Task<List<BatchProcess>> GetByBatch(int batch)
    {
        try
        {
            var result = await _context.BatchProcesses
                .Where(p => p.Batch == batch)
                .OrderBy(p => p.StartTime)
                .ToListAsync();

            _diagnostics.LogCommand($"SELECT BatchProcess WHERE Batch = {batch}", true, null, nameof(BatchProcessRepository));
            return result;
        }
        catch (Exception ex)
        {
            _diagnostics.LogCommand($"SELECT BatchProcess WHERE Batch = {batch}", false, ex.Message, nameof(BatchProcessRepository));
            throw;
        }
    }

    public async Task UpdateAsync(BatchProcess batch)
    {
        try
        {
            _context.BatchProcesses.Update(batch);
            await _context.SaveChangesAsync();
            _diagnostics.LogCommand("UPDATE BatchProcess", true, null, nameof(BatchProcessRepository));
        }
        catch (Exception ex)
        {
            _diagnostics.LogCommand("UPDATE BatchProcess", false, ex.Message, nameof(BatchProcessRepository));
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var batch = await _context.BatchProcesses.FindAsync(id);
            if (batch is not null)
            {
                _context.BatchProcesses.Remove(batch);
                await _context.SaveChangesAsync();
                _diagnostics.LogCommand($"DELETE BatchProcess WHERE Id = {id}", true, null, nameof(BatchProcessRepository));
            }
            else
            {
                _diagnostics.LogCommand($"DELETE BatchProcess WHERE Id = {id}", false, "Not found", nameof(BatchProcessRepository));
            }
        }
        catch (Exception ex)
        {
            _diagnostics.LogCommand($"DELETE BatchProcess WHERE Id = {id}", false, ex.Message, nameof(BatchProcessRepository));
            throw;
        }
    }
}
