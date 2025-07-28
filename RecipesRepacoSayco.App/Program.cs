using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RecipesRepacoSayco.App.Data;
using RecipesRepacoSayco.App.Models;
using RecipesRepacoSayco.Data.Data;
using RecipesRepacoSayco.Data.Repositories;
using Microsoft.Extensions.Options;
using RecipesRepacoSayco.App.Services;
using RecipesRepacoSayco.Core.Services;
using RecipesRepacoSayco.App.Hubs;
using RecipesRepacoSayco.Core.Models;
using RecipesRepacoSayco.Plc.Managers;
using RecipesRepacoSayco.Infraestructure.Managers;
using RecipesRepacoSayco.Infraestructure.Services;
using RecipesRepacoSayco.Core.Models.Definitions;
using RecipesRepacoSayco.Core.Models.Reports;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddMudServices();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();


builder.Services.AddLocalization(options => options.ResourcesPath = "");

// Register EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var labels = builder.Configuration.GetSection("MaterialLabels").Get<string[]>();
builder.Services.Configure<MaterialLabelOptions>(options =>
{
    options.MaterialLabels = builder.Configuration
        .GetSection("MaterialLabels")
        .Get<List<string>>() ?? new();
});

builder.Services.AddSingleton<PlcStatusService>(new PlcStatusService());

// Register repositories
builder.Services.AddScoped<IBatchProcessRepository, BatchProcessRepository>();
builder.Services.AddScoped<IExcelReportService, ExcelReportService>();

builder.Services.AddSingleton<ITagNotifier, SignalRTagNotifier>(); // O NullTagNotifier si no usás SignalR

builder.Services.AddSingleton<PlcManager>(sp =>
{
    var notifier = sp.GetRequiredService<ITagNotifier>();
    var plcDefinitions = builder.Configuration
.GetSection("PlcConnections")
.Get<IEnumerable<PlcConnectionDefinition>>();

    var manager = new PlcManager(notifier, plcDefinitions);
    manager.InitializePlcs(); // crea e instancia los PLCs
    return manager;
});

Console.WriteLine("Creating ReportManager ======================================");
builder.Services.Configure<ReportMappingConfig>(
    builder.Configuration.GetSection("ReportMapping"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ReportMappingConfig>>().Value);


builder.Services.AddScoped<ReportManager>();
builder.Services.AddSingleton<ReportRunner>();

var app = builder.Build();
_ = app.Services.GetRequiredService<ReportRunner>();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();
app.MapHub<PlcHub>("/plcHub");


var supportedCultures = new[] { "en-US", "es-ES" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.Run();
