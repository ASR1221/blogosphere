using Blogosphere.API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = builder.Configuration["MySQLDbConnectionString"] ?? "";

builder.Services.AddDbContext<AppDbContext>(
	dbContextOptions => dbContextOptions
		.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
		// TODO: should be changed or removed for production.
		.LogTo(Console.WriteLine, LogLevel.Warning)
		.EnableSensitiveDataLogging()
		.EnableDetailedErrors()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
