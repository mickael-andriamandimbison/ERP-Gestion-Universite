using Scalar.AspNetCore;
using erp_univ_back.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Shared.Middlewares;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ErpUnivDbContext>((opt) => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Repository") || c.Name.EndsWith("Generator")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddOpenApi();

ConfigureMapster();
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddMapster();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();

app.Run();

public partial class Program
{
    private static bool _isMapsterConfigured;
    private static readonly object _mapsterLock = new();

    public static void ConfigureMapster()
    {
        if (!_isMapsterConfigured)
        {
            lock (_mapsterLock)
            {
                if (!_isMapsterConfigured)
                {
                    TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
                    _isMapsterConfigured = true;
                }
            }
        }
    }
}
