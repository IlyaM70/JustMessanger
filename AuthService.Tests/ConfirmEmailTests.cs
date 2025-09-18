using AuthService.Controllers;
using AuthService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AuthService.Tests
{
	public class ConfirmEmailTests
	{
		#region ConfirmEmail_Fails_UserIdMissing
		[Fact]
		public async Task ConfirmEmail_Fails_UserIdMissing()
		{
			//Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			await using var dbContext = new AuthDbContext(options);


			//Act
			var controller = new AuthController(dbContext, null, null, null);
			var result = controller.ConfirmEmail(null, "sometoken");
			var result1 = controller.ConfirmEmail("", "sometoken");
			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.IsType<BadRequestObjectResult>(result1);

			Assert.Equal("User ID and Token are required", (result as BadRequestObjectResult).Value);
			Assert.Equal("User ID and Token are required", (result1 as BadRequestObjectResult).Value);

		}
		#endregion

		#region ConfirmEmail_Fails_TokenMissing
		[Fact]
		public async Task ConfirmEmail_Fails_TokenMissing()
		{
			//Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			await using var dbContext = new AuthDbContext(options);


			//Act
			var controller = new AuthController(dbContext, null, null, null);
			var result = controller.ConfirmEmail("someid", "");
			var result1 = controller.ConfirmEmail("someid", null);
			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.IsType<BadRequestObjectResult>(result1);

			Assert.Equal("User ID and Token are required", (result as BadRequestObjectResult).Value);
			Assert.Equal("User ID and Token are required", (result1 as BadRequestObjectResult).Value);

		}
		#endregion

		#region ConfirmEmail_Fails_UserNotFound
		[Fact]
		public async Task ConfirmEmail_Fails_UserNotFound()
		{
			//Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			await using var dbContext = new AuthDbContext(options);
			dbContext.Users
				.Add(new ApplicationUser { Id = "existingid",
					Email = "existinguser@mail.com" });
			await dbContext.SaveChangesAsync();


			//Act
			var controller = new AuthController(dbContext, null, null, null);
			var result = controller.ConfirmEmail("someid", "sometoken");
			//Assert
			Assert.IsType<BadRequestObjectResult>(result);

			Assert.Equal("User was not found",
				(result as BadRequestObjectResult).Value);

		}
		#endregion

		#region ConfirmEmail_Fails_ConfirmEmailAsyncFails
		[Fact]
		public async Task ConfirmEmail_Fails_ConfirmEmailAsyncFails()
		{		
			// Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			await using var dbContext = new AuthDbContext(options);

			var existingUser = new ApplicationUser
			{
				Id = "existingid",
				Email = "existinguser@mail.com"
			};
			dbContext.Users.Add(existingUser);
			await dbContext.SaveChangesAsync();

			var storeMock = new Mock<IUserStore<ApplicationUser>>();
			var userManagerMock = new Mock<UserManager<ApplicationUser>>(
				storeMock.Object, null, null, null, null, null, null, null, null);

			// Ensure FindByIdAsync returns the user from the mock UserManager
			userManagerMock
				.Setup(um => um.FindByIdAsync(existingUser.Id))
				.ReturnsAsync(existingUser);

			// Simulate ConfirmEmailAsync failure
			userManagerMock
				.Setup(um => um.ConfirmEmailAsync(existingUser, "sometoken"))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

			// Act
			var controller = new AuthController(dbContext, userManagerMock.Object, null, null);
			var result = controller.ConfirmEmail(existingUser.Id, "sometoken");

			// Assert - controller should treat a failed ConfirmEmailAsync as a bad request
			Assert.IsType<BadRequestObjectResult>(result);
		}
		#endregion

		#region ConfirmEmail_Success_ValidUserAndToken
		[Fact]
		public async Task ConfirmEmail_Success_ValidUserAndToken()
		{
			//Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			await using var dbContext = new AuthDbContext(options);

			var existingUser = new ApplicationUser
			{
				Id = "existingid",
				Email = "existinguser@mail.com"
			};
			dbContext.Users.Add(existingUser);
			await dbContext.SaveChangesAsync();

			var storeMock = new Mock<IUserStore<ApplicationUser>>();
			var userManagerMock = new Mock<UserManager<ApplicationUser>>(
				storeMock.Object, null, null, null, null, null, null, null, null);

			// Ensure FindByIdAsync returns the user from the mock UserManager
			userManagerMock
				.Setup(um => um.FindByIdAsync(existingUser.Id))
				.ReturnsAsync(existingUser);

			// set up ConfirmEmailAsync 
			userManagerMock
				.Setup(um => um.ConfirmEmailAsync(existingUser, "sometoken"))
				.ReturnsAsync(IdentityResult.Success);
			//Act
			var controller = new AuthController(dbContext, userManagerMock.Object, null, null);
			var result = controller.ConfirmEmail("existingid", "sometoken");
			//Assert
			Assert.IsType<OkObjectResult>(result);	
		}
		#endregion
	}
}
