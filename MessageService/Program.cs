using MessageService;
using MessageService.Data;
using MessageService.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Authentication
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

builder.Services.AddAuthorization();


builder.Services.AddDbContext<MessageDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpClient<AuthorizationClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135");
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7097")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(); // Call this *before* app.UseAuthorization()

app.UseAuthentication(); // MUST come before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessagesHub>("/messagesHub");

app.Run();
