using AuthService.Controllers;
using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
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
		class EmailConfirmatorMock : IEmailConfirmator
		{
			public Task SendConfirmationEmailAsync(ApplicationUser user, string confirmationLink, string baseUrl)
			{
				// Mock implementation does nothing
				return Task.CompletedTask;
			}
		}

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
			EmailConfirmatorMock emailConfirmator = new();
			AuthController controller = new(authDbContext, userManagerMock.Object, configuration, emailConfirmator);

			RegisterRequest request = new RegisterRequest
			{
				UserName = "testuser",
				Email = "test@test.com",
				Password = "Password123!"
			};

			//end arrange
				#endregion

				// Act
			var result = await controller.Register(request);
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

		#region Register_Fails_EmailInUse
		[Fact]
		public async void Register_Fails_EmailInUse()
		{
			#region Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: $"AuthTestDb_Fails_EmailInUse")
				.Options;
			var authDbContext = new AuthDbContext(options);

			// Seed the in-memory database with an existing user
			authDbContext.Users.Add(new ApplicationUser { UserName = "existinguser", Email = "existinguser@mail.com" });
			authDbContext.SaveChanges();


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
			EmailConfirmatorMock emailConfirmator = new();
			AuthController controller = new(authDbContext, userManagerMock.Object, configuration, emailConfirmator);

			RegisterRequest request = new RegisterRequest
			{
				UserName = "newuser",
				Email = "existinguser@mail.com",
				Password = "Newuser123!"
			};

			//end arrange
			#endregion

			// Act
			var result = await controller.Register(request);

			// Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Email already in use.", (result as BadRequestObjectResult).Value);

		}
		#endregion

		#region Register_Fails_PasswordTooWeak
		[Fact]
		public async void Register_Fails_PasswordTooWeak()
		{
			#region Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: $"AuthTestDb_Fails_PasswordTooWeak")
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
			// Setup CreateAsync to return a failed IdentityResult indicating weak password
			userManagerMock
				.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password is too weak." }));
			IConfiguration configuration = new ConfigurationManager();
			EmailConfirmatorMock emailConfirmator = new();
			AuthController controller = new(authDbContext, userManagerMock.Object, configuration, emailConfirmator);

			RegisterRequest request = new RegisterRequest
			{
				UserName = "weakuser",
				Email = "weakuser@mail.com",
				Password = "weak"
			};

			//end arrange
			#endregion
			// Act
			var result = await controller.Register(request);
			// Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Error:  - Password is too weak.\n", (result as BadRequestObjectResult).Value);
		}
		#endregion

		#region Register_Success_SendsConfirmationEmail
		[Fact]
		public async void Register_Success_SendsConfirmationEmail()
		{
			#region Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: $"AuthTestDb_Success_SendsConfirmationEmail")
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

			// Setup GenerateEmailConfirmationTokenAsync to return a known token
			const string expectedToken = "EXPECTED_CONFIRM_TOKEN";
			userManagerMock
				.Setup(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(expectedToken);

			IConfiguration configuration = new ConfigurationManager();

			// Use a Mock<IEmailConfirmator> so we can verify the call and its arguments
			var emailConfirmatorMock = new Mock<IEmailConfirmator>();
			emailConfirmatorMock
				.Setup(ec => ec.SendConfirmationEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Task.CompletedTask);

			AuthController controller = new(authDbContext, userManagerMock.Object, configuration, emailConfirmatorMock.Object);

			RegisterRequest request = new RegisterRequest
			{
				UserName = "testuser",
				Email = "test@test.com",
				Password = "Password123!"
			};

			//end arrange
			#endregion

			// Act
			var result = await controller.Register(request);

			// Assert result is success
			Assert.IsType<OkObjectResult>(result);

			// Verify CreateAsync was invoked once
			userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);

			// Verify GenerateEmailConfirmationTokenAsync was invoked with the created user
			userManagerMock.Verify(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);

			// Verify SendConfirmationEmailAsync was invoked exactly once with the created user and the token we returned
			emailConfirmatorMock.Verify(ec =>
				ec.SendConfirmationEmailAsync(
					It.Is<ApplicationUser>(u => u != null && (u.UserName == "testuser" || u.Email == "test@test.com")),
					expectedToken,
					It.IsAny<string>()),
				Times.Once);
			

		}
		#endregion
	}
}