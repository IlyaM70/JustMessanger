using AuthService.Controllers;
using AuthService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;


namespace AuthService.Tests
{
	public class RegisterTests
	{
		#region Register_CreatesUser_WhenInputIsValid
		[Fact]
		public async void Register_CreatesUser_WhenInputIsValid()
		{
			#region Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: $"AuthTestDb_CreatesUser_WhenInputIsValid")
				.Options;
			var authDbContext = new AuthDbContext(options);

			// Create a mock IUserStore required by UserManager
			var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

			// Create UserManager mock
			var userManagerMock = new Mock<UserManager<ApplicationUser>>(
				userStoreMock.Object,
				null, // IOptions<IdentityOptions>
				null, // IPasswordHasher<TUser>
				null, // IEnumerable<IUserValidator<TUser>>
				null, // IEnumerable<IPasswordValidator<TUser>>
				null, // ILookupNormalizer
				null, // IdentityErrorDescriber
				null, // IServiceProvider
				null  // ILogger<UserManager<TUser>>
			);

			// Setup CreateAsync to add the created user to the in-memory DbContext and return success
			userManagerMock
				.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success)
				.Callback<ApplicationUser, string>((user, pwd) =>
				{
					// Ensure we set username/email on the user instance
					if (user != null)
					{
						// Add to in-memory DB to simulate persistence
						authDbContext.Users.Add(user);
						authDbContext.SaveChanges();
					}
				});

			IConfiguration configuration = new ConfigurationManager();
			AuthController controller = new(authDbContext, userManagerMock.Object, configuration);
			
			//end arrange
			#endregion


			// Act
			var result = await controller.Register("testuser", "test@test.com", "Password123!");
			// Assert
			Assert.IsType<OkObjectResult>(result);

			// verify the user was created in the database
			var created = authDbContext.Users.SingleOrDefault(u => u.UserName == "testuser" || u.Email == "test@test.com");
			Assert.NotNull(created);
			Assert.Equal("test@test.com", created.Email);

			// Verify CreateAsync was invoked once
			userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
		}
		#endregion
	}
}