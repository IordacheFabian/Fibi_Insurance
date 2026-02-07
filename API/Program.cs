
using Application.Clients.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using MediatR;
using Application;
using Application.Core;
using FluentValidation;
using Persistence.Repositories.Clients;
using Application.Core.Interfaces.IRepositories;
using Persistence.Repositories;
using Application.Clients.DTOs.Validators;
using API.Middleware;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ExceptionMiddleware>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors();

builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IGeographyRepository, GeographyRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IMetadataRepository, MetadataRepository>();
builder.Services.AddScoped<IPremiumCalculator, PremiumCalculator>();

builder.Services.AddAutoMapper(x => {}, typeof(MappingProfiles).Assembly);

builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
// builder.Services.AddValidatorsFromAssemblyContaining<CreateClientDtoValidator>();

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), 
    typeof(ValidationBehavior<,>)
);
// Lowercase routes
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
);


app.MapControllers();

app.Run();