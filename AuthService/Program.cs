using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options=>
{
	options.SignIn.RequireConfirmedEmail = true;
})
	.AddEntityFrameworkStores<AuthDbContext>()
	.AddDefaultTokenProviders();

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
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	};
});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IEmailConfirmator, EmailConfirmator>();

var app = builder.Build();

//seed users
using (var scope = app.Services.CreateScope())
{
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	// ensure DB is created
	var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
	db.Database.Migrate();

	// create user if not exists
	var bob = await userManager.FindByEmailAsync("bob@test.com");
	if (bob == null)
	{
		var user = new ApplicationUser
		{
			UserName = "Bob",
			Email = "bob@test.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(user, "Bob123!");

	}

	var charlie = await userManager.FindByEmailAsync("charlie@test.com");
	if (charlie == null)
	{
		var user = new ApplicationUser
		{
			UserName = "Charlie",
			Email = "charlie@test.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(user, "Charlie123!");

	}

	var karla = await userManager.FindByEmailAsync("karla@test.com");
	if (karla == null)
	{
		var user = new ApplicationUser
		{
			UserName = "Karla",
			Email = "karla@test.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(user, "Karla123!");

	}
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
