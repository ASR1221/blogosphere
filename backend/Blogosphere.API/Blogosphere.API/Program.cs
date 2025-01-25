using System.Text;
using Blogosphere.API.Middlewares;
using Blogosphere.API.Models;
using Blogosphere.API.Models.Entities;
using Blogosphere.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(option =>  
{ 
 	option.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Blogosphere API",
		Version = "v1",
		Description = "Blogosphere API with custom JWT authentication"
	});

	// Configure JWT authentication for Swagger
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = @"JWT Authorization header using the Bearer scheme. 
							Enter your token in the text input below.
							Example: 'Bearer 12345abcdef'",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
			{
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "Bearer"
					},
					Scheme = "Bearer",
					Name = "Bearer",
					In = ParameterLocation.Header
				},
				new List<string>()
			}
	});
});


string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "";

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

		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(double.Parse(Environment.GetEnvironmentVariable("JwtExpireDays") ?? "7"));
		options.Lockout.MaxFailedAccessAttempts = 5;

		options.SignIn.RequireConfirmedAccount = false;
		options.SignIn.RequireConfirmedEmail = false;
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
		ValidIssuer = Environment.GetEnvironmentVariable("JwtIssuer") ?? "a",
		ValidAudience = Environment.GetEnvironmentVariable("JwtAudience") ?? "a",
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtKey") ?? "randomKeysoThatNoErrorIsThrownIfENVIsNotSet123456789abcdefgh")),
	};
});

builder.Services.AddResponseCaching();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IBlogsService, BlogsService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ILikesService, LikesService>();
builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
	app.UseSwagger();
	app.UseSwaggerUI();
// }

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

app.UseJwtValidation();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.MapControllers();

app.Run();
