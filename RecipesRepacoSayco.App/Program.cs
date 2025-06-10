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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddMudServices();
builder.Services.AddHttpClient();


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


var app = builder.Build();



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

var supportedCultures = new[] { "en-US", "es-ES" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.Run();
