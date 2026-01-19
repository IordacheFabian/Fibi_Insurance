
using Application.Clients.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using MediatR;
using Application;
using Application.Core;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors();

builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});

builder.Services.AddAutoMapper(x => {}, typeof(MappingProfiles).Assembly);

// builder.Services.AddSwaggerGen();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.MapControllers();

app.Run();