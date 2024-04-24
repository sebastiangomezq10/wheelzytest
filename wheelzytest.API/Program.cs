using Microsoft.EntityFrameworkCore;
using wheelzytest.Infrastructure.Context;
using Microsoft.OpenApi.Models;
using wheelzytest.Domain.Interfaces;
using wheelzytest.Infrastructure.Repositories;
using wheelzytest.Core.Interfaces;
using wheelzytest.Core.Services;
using wheelzytest.Domain.Models;
using wheelzytest.Infrastructure.DataInitialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ICarRepository, CarRepository>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Wheelzy API", Version = "v1" });
});

// Database using SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
   {
       options.UseInMemoryDatabase(databaseName: "InMemoryCarDatabase");
   });

// CORS Configuration
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", b =>
{
    b.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader();
}));

var app = builder.Build();

// Apply EF Core migrations and initialize data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    DatabaseInitializer.Initialize(dbContext);     // Optional: Initialize data if required
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wheelzy API v1"));
}

app.UseCors("MyPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



