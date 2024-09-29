using System.Text;
using Blogosphere.API.Models;
using Blogosphere.API.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Append services to the container.

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

// set up auth and auth options
builder.Services.AddIdentity<User, IdentityRole>(options =>
	{
		// Password settings
		options.Password.RequiredLength = 8;
		options.Password.RequireDigit = true;
		options.Password.RequireLowercase = true;
		options.Password.RequireUppercase = true;
		options.Password.RequireNonAlphanumeric = false;

		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
		options.Lockout.MaxFailedAccessAttempts = 5;
		
		options.SignIn.RequireConfirmedAccount = true;
	}
)
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// set up jwt
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["JwtIssuer"],
		ValidAudience = builder.Configuration["JwtAudience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtKey"] ?? ""))
	};
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHsts();
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
	context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
	context.Response.Headers.Append("X-Frame-Options", "DENY");
	context.Response.Headers.Append("X-XSS-Protection", "0");
	context.Response.Headers.Append("Cross-Origin-Resource-Policy", "same-origin");
	context.Response.Headers.Append("Content-Security-Policy", "default-src 'self';base-uri 'self';font-src 'self' https: data:;form-action 'self';frame-ancestors 'self';img-src 'self' data:;object-src 'none';script-src 'self';script-src-attr 'none';style-src 'self' https: 'unsafe-inline';upgrade-insecure-requests");
	await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
