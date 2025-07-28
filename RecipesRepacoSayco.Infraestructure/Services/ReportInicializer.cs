using System;
using Microsoft.Extensions.DependencyInjection;
using RecipesRepacoSayco.Infraestructure.Managers;

namespace RecipesRepacoSayco.Infraestructure.Services;

public class ReportRunner
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportRunner(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        Task.Run(RunAsync);
    }

    private async Task RunAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var reportManager = scope.ServiceProvider.GetRequiredService<ReportManager>();
        await reportManager.StartCycleAsync();
    }
}

