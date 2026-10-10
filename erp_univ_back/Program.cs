using Scalar.AspNetCore;
using erp_univ_back.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ErpUnivDbContext>((opt) => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Repository") || c.Name.EndsWith("Generator")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
builder.Services.AddOpenApi();


var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
typeAdapterConfig.Scan(typeof(Program).Assembly);

builder.Services.AddSingleton(typeAdapterConfig);
builder.Services.AddMapster();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();
