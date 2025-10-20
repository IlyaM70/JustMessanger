using System;
using System.Text;
using MessageService;
using MessageService.Data;
using MessageService.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// register CORS policy
var serviceUrls = new[]
{
	builder.Configuration["WebClient:BaseUrl"],
	builder.Configuration["MessageService:BaseUrl"],
	builder.Configuration["AuthService:BaseUrl"]
};

builder.Services.AddCors(options =>
{
	options.AddPolicy("DefaultPolicy", policy =>
	{
		policy.WithOrigins(serviceUrls.Where(u => !string.IsNullOrEmpty(u)).ToArray())
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});



builder.Services.AddControllers();

#region Add Authentication
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false; // set true in production
	options.SaveToken = true;

	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,

		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	};

	// Helpful debugging: log reasons why token validation fails
	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = ctx =>
		{
			var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
			logger.LogError(ctx.Exception, "JWT Authentication failed: {message}", ctx.Exception.Message);
			return Task.CompletedTask;
		},
		OnTokenValidated = ctx =>
		{
			var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
			logger.LogInformation("JWT validated for {sub}", ctx.Principal?.Identity?.Name);
			return Task.CompletedTask;
		}
	};
});
#endregion
builder.Services.AddAuthorization();

	
builder.Services.AddDbContext<MessageDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default"),
	sqlOptions => sqlOptions.EnableRetryOnFailure())
	);

builder.Services.AddHttpClient<AuthorizationClient>(client =>
{
	var baseUrl = builder.Configuration["AuthService:BaseUrl"];
	client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"{token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] { }
        }
    });
});

var app = builder.Build();

//Apply migrations
try
{
	using var scope = app.Services.CreateScope();
	var db = scope.ServiceProvider.GetRequiredService<MessageDbContext>();
	db.Database.Migrate();
}
catch (Exception ex)
{
	Console.WriteLine($"Migration failed: {ex.Message}");
}



app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("DefaultPolicy"); // Call this *before* app.UseAuthorization()

app.UseAuthentication(); // MUST come before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessagesHub>("/messagesHub");

app.Run();
