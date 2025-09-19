using AuthService.Controllers;
using AuthService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Tests
{
	public class UserExistTests
	{
		#region UserExist_ReturnsNotFound_WhenUserDoesNotExist
		[Fact]
		public async Task UserExist_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			await using var dbContext = new AuthDbContext(options);

			
			var controller = new AuthController(dbContext, null, null, null);

			// Act
			var result = controller.UserExist("nonexistentId");

			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal("User was not found", notFoundResult.Value);
		}
		#endregion

		#region UserExist_ReturnsOk_WhenUserExists
		[Fact]
		public async Task UserExist_ReturnsOk_WhenUserExists()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<AuthDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			await using var dbContext = new AuthDbContext(options);

			var seededUser = new ApplicationUser
			{
				Id = "existing-user-id",
				Email = "user@example.com",
				UserName = "user"
			};

			dbContext.Users.Add(seededUser);
			await dbContext.SaveChangesAsync();

			var controller = new AuthController(dbContext, null, null, null);

			// Act
			var result = controller.UserExist(seededUser.Id);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Equal("User found", okResult.Value);
		}
		#endregion	

	}
}
