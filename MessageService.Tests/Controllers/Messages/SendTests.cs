using MessageService.Controllers;
using MessageService.Data;
using MessageService.Hubs;
using MessageService.Models;
using MessageService.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;

namespace MessageService.Tests.Controllers.Messages
{
	public class SendTests
	{
		#region Send_Should_AddMessageToDatabase()
		[Fact]
		public async Task Send_Should_AddMessageToDatabase()
		{
			#region Arrange
			//Arrange: create in‑memory DbContext
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("PersistTestDb")
				.Options;
			await using var db = new MessageDbContext(options);

			await db.Users.AddAsync(new User { Id = "1", Username = "Sender" });
			await db.Users.AddAsync(new User { Id = "2", Username = "Recipient" });
			await db.SaveChangesAsync();

			//Arrange: mock IHubContext
			var hubContext = new Mock<IHubContext<MessagesHub>>();
			var clients = new Mock<IHubClients>();
			var clientProxy = new Mock<IClientProxy>();

			clients
				.Setup(c => c.Group(It.IsAny<string>()))
				.Returns(clientProxy.Object);

			hubContext
				.Setup(h => h.Clients)
				.Returns(clients.Object);

			clientProxy
				.Setup(p => p.SendCoreAsync(
					It.IsAny<string>(),
					It.IsAny<object[]>(),
					It.IsAny<CancellationToken>())
				)
				.Returns(Task.CompletedTask);


			//Arrange controller and message dto
			var controller = new MessageController(db, hubContext.Object);

			var dto = new SendMessageDto
			{
				SenderId = "1",
				RecipientId = "2",
				Text = "Hello Persistence"
			};
			#endregion

			//Act: call Send()
			var result = await controller.Send(dto);

			// Assert: database contains exactly one message
			var messages = await db.Messages.ToListAsync();
			Assert.Single(messages);

			var saved = messages[0];
			Assert.Equal("1", saved.SenderId);
			Assert.Equal("2", saved.RecipientId);
			Assert.Equal("Hello Persistence", saved.Text);

			//Assert: SentAt is recent (within last 5 seconds)
			Assert.InRange((DateTime.UtcNow - saved.SentAt).TotalSeconds, 0, 5);
		}
		#endregion

		#region Send_Should_SendMessageViaSignalR_WithCorrectPayload()
		[Fact]
		public async Task Send_Should_SendMessageViaSignalR_WithCorrectPayload()
		{
			#region Arrange
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("SignalRTestDb")
				.Options;
			await using var db = new MessageDbContext(options);

			await db.Users.AddAsync(new User { Id = "1", Username = "Sender" });
			await db.Users.AddAsync(new User { Id = "2", Username = "Recipient" });
			await db.SaveChangesAsync();

			var hubContext = new Mock<IHubContext<MessagesHub>>();
			var clients = new Mock<IHubClients>();
			var clientProxy = new Mock<IClientProxy>();

			object[]? capturedPayload = null;

			clientProxy
				.Setup(p => p.SendCoreAsync(
					"ReceiveMessage",
					It.IsAny<object[]>(),
					It.IsAny<CancellationToken>()))
				.Callback<string, object[], CancellationToken>((_, args, _) =>
				{
					capturedPayload = args;
				})
				.Returns(Task.CompletedTask);

			clients
				.Setup(c => c.Group("2"))
				.Returns(clientProxy.Object);

			hubContext
				.Setup(h => h.Clients)
				.Returns(clients.Object);

			var controller = new MessageController(db, hubContext.Object);

			var dto = new SendMessageDto
			{
				SenderId = "1",
				RecipientId = "2",
				Text = "Hello Test"
			};
			#endregion

			// Act
			var result = await controller.Send(dto);

			// Assert: SignalR push happened once to group "2"
			clients.Verify(c => c.Group("2"), Times.Once);
			clientProxy.Verify(p =>
				p.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
				Times.Once);

			// Assert: payload was captured and contains expected values
			Assert.NotNull(capturedPayload);
			Assert.Single(capturedPayload);

			var payload = capturedPayload![0]!;
			var json = JsonSerializer.Serialize(payload);
			var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

			Assert.NotNull(deserialized);
			Assert.Equal("1", deserialized!["SenderId"].GetString());
			Assert.Equal("Hello Test", deserialized["Text"].GetString());
			Assert.True(deserialized["Id"].GetInt32() > 0);
			Assert.True(deserialized["SentAt"].GetDateTime() <= DateTime.UtcNow);
		}
		#endregion

		#region Send_Should_Return_OkResult_On_Success()
		[Fact]
		public async Task Send_Should_Return_OkResult_On_Success()
		{
			#region Arrange
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("TestDb_OkReturn")
				.Options;

			await using var db = new MessageDbContext(options);

			await db.Users.AddAsync(new User { Id = "1", Username = "Sender" });
			await db.Users.AddAsync(new User { Id = "2", Username = "Recipient" });
			await db.SaveChangesAsync();

			var hubContext = new Mock<IHubContext<MessagesHub>>();
			var clients = new Mock<IHubClients>();
			var clientProxy = new Mock<IClientProxy>();

			clients
				.Setup(c => c.Group(It.IsAny<string>()))
				.Returns(clientProxy.Object);

			hubContext
				.Setup(h => h.Clients)
				.Returns(clients.Object);

			clientProxy
				.Setup(p => p.SendCoreAsync(
					It.IsAny<string>(),
					It.IsAny<object[]>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			var controller = new MessageController(db, hubContext.Object);

			var dto = new SendMessageDto
			{
				SenderId = "1",
				RecipientId = "2",
				Text = "Testing return value"
			};
			#endregion

			// Act
			var result = await controller.Send(dto);

			// Assert
			Assert.IsType<OkResult>(result);
		}
		#endregion
	}
}
