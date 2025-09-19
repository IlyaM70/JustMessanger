using AuthService.Controllers;
using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthService.Tests
{
	public class LoginTests
	{
		#region Login_Fails_UserNotFound
		[Fact]
		public async void Login_Fails_UserNotFound()
		{
			//Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			await using var dbContext = new AuthDbContext(options);

			dbContext.Users
				.Add(new ApplicationUser()
				{
					Id = Guid.NewGuid().ToString(),
					Email = "existinguser",
					PasswordHash = Guid.NewGuid().ToString(),
				});


			//Act
			var controller = new AuthController(dbContext, null, null, null);
			var result = controller.Login("nonexistinguser", "password");

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid email or password.", (result as BadRequestObjectResult).Value);

		}
		#endregion

		#region Login_Fails_PasswordIncorrect
		[Fact]
		public async Task Login_Fails_PasswordIncorrect()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			await using var dbContext = new AuthDbContext(options);

			var existingUser = new ApplicationUser
			{
				Id = Guid.NewGuid().ToString(),
				Email = "existinguser@example.com",
				UserName = "existinguser"
			};

			dbContext.Users.Add(existingUser);
			await dbContext.SaveChangesAsync();

			var storeMock = new Mock<IUserStore<ApplicationUser>>();
			var userManagerMock = new Mock<UserManager<ApplicationUser>>(
				storeMock.Object, null, null, null, null, null, null, null, null);

			// Setup CheckPasswordAsync to return false for the user with this email and the specific password
			userManagerMock
				.Setup(um => um.CheckPasswordAsync(
					It.Is<ApplicationUser>(u => u.Email == "existinguser@example.com"),
					"password"))
				.ReturnsAsync(false);

			var emailConfirmatorMock = new Mock<IEmailConfirmator>();
			IConfiguration configuration = new ConfigurationManager();

			// Use the controller constructor that matches your code (4 params shown here)
			var controller = new AuthController(dbContext, userManagerMock.Object, configuration, emailConfirmatorMock.Object);

			// Act
			var result = controller.Login("existinguser@example.com", "password");

			// Assert response
			Assert.IsType<BadRequestObjectResult>(result);
			var badReq = result as BadRequestObjectResult;
			Assert.NotNull(badReq);
			Assert.Equal("Invalid email or password.", badReq.Value);

			// Verify CheckPasswordAsync was called once with expected user (matched by email) and password
			userManagerMock.Verify(um => um.CheckPasswordAsync(
				It.Is<ApplicationUser>(u => u.Email == "existinguser@example.com"),
				"password"), Times.Once);
		}

		#endregion

		#region Login_Succeed_ValidCredentials
		[Fact]
		public async Task Login_Succeed_ValidCredentials()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			await using var dbContext = new AuthDbContext(options);

			var existingUser = new ApplicationUser
			{
				Id = Guid.NewGuid().ToString(),
				Email = "existinguser@example.com",
				UserName = "existinguser"
			};

			dbContext.Users.Add(existingUser);
			await dbContext.SaveChangesAsync();

			// Mock UserStore and UserManager
			var storeMock = new Mock<IUserStore<ApplicationUser>>();
			var userManagerMock = new Mock<UserManager<ApplicationUser>>(
				storeMock.Object, null, null, null, null, null, null, null, null);

			// When CheckPasswordAsync is called for this user and password, return true
			userManagerMock
				.Setup(um => um.CheckPasswordAsync(
					It.Is<ApplicationUser>(u => u.Email == "existinguser@example.com"),
					"Password123!"))
				.ReturnsAsync(true);

			// In-memory configuration for JWT generation (must match what GenerateToken reads)
			var inMemorySettings = new Dictionary<string, string>
			{
				["Jwt:Key"] = "super_secret_jwt_key_which_is_long_enough",
				["Jwt:Issuer"] = "TestIssuer",
				["Jwt:Audience"] = "TestAudience"
			};
			IConfiguration configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(inMemorySettings)
				.Build();

			var emailConfirmatorMock = new Mock<IEmailConfirmator>();

			var controller = new AuthController(dbContext, userManagerMock.Object, configuration, emailConfirmatorMock.Object);

			// Act
			var actionResult = controller.Login("existinguser@example.com", "Password123!");

			// Assert result shape
			var okResult = Assert.IsType<OkObjectResult>(actionResult);
			Assert.NotNull(okResult.Value);

			// Extract token from anonymous object: new { token }
			var tokenObj = okResult.Value;
			var tokenProp = tokenObj.GetType().GetProperty("token");
			Assert.NotNull(tokenProp);
			var token = tokenProp.GetValue(tokenObj) as string;
			Assert.False(string.IsNullOrEmpty(token), "Token should not be null or empty");

			// Validate token signature and get validated token
			var tokenHandler = new JwtSecurityTokenHandler();
			var keyBytes = Encoding.UTF8.GetBytes(inMemorySettings["Jwt:Key"]);

			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
				ValidateIssuer = true,
				ValidIssuer = inMemorySettings["Jwt:Issuer"],
				ValidateAudience = true,
				ValidAudience = inMemorySettings["Jwt:Audience"],
				ValidateLifetime = false // disable lifetime validation to avoid time skew in tests
			};

			var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
			Assert.NotNull(principal);

			var jwt = validatedToken as JwtSecurityToken;
			Assert.NotNull(jwt);

			// Assert exact claim types created by GenerateToken
			var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
			var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
			var uidClaim = jwt.Claims.FirstOrDefault(c => c.Type == "uid");
			var jtiClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

			Assert.NotNull(subClaim);
			Assert.Equal(existingUser.UserName, subClaim.Value);

			Assert.NotNull(emailClaim);
			Assert.Equal(existingUser.Email, emailClaim.Value);

			Assert.NotNull(uidClaim);
			Assert.Equal(existingUser.Id, uidClaim.Value);

			Assert.NotNull(jtiClaim); // jti exists but value is random

			// Check expiry ~7 days from now (allow small window)
			var daysUntilExpiry = (jwt.ValidTo - DateTime.UtcNow).TotalDays;
			Assert.InRange(daysUntilExpiry, 6.5, 7.5);

			// Verify CheckPasswordAsync was invoked once
			userManagerMock.Verify(um => um.CheckPasswordAsync(
				It.Is<ApplicationUser>(u => u.Email == "existinguser@example.com"),
				"Password123!"), Times.Once);
		}
		#endregion

	}
}
