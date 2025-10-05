using MessageService.Controllers;
using MessageService.Data;
using MessageService.Hubs;
using MessageService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using System.Text.Json;

namespace MessageService.Tests
{
	public class SendTests
	{
		// test double for AuthorizationClient
		private class TestAuthorizationClient : AuthorizationClient
		{
			private readonly HashSet<string> _existingUserIds;
			public TestAuthorizationClient(IEnumerable<string> existingUserIds)
				: base(new HttpClient()) // base HttpClient not used by the test double
			{
				_existingUserIds = new HashSet<string>(existingUserIds);
			}
			public override Task<bool> IsUserExistAsync(string userId)
			{
				return Task.FromResult(_existingUserIds.Contains(userId));
			}
		}

		private MessageController CreateController(MessageDbContext db, Mock<IHubContext<MessagesHub>> hubContext, AuthorizationClient authClient, string senderId = "1")
		{
			var controller = new MessageController(db, hubContext.Object, authClient);
			var user = new ClaimsPrincipal(
				new ClaimsIdentity(
					new[] { new Claim("uid", senderId) }
				)
			);
			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = user }
			};
			return controller;
		}

		[Fact]
		public async Task Send_Should_AddMessageToDatabase()
		{
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("PersistTestMessagesDb")
				.Options;
			await using var db = new MessageDbContext(options);

			var hubContext = new Mock<IHubContext<MessagesHub>>();
			var clients = new Mock<IHubClients>();
			var clientProxy = new Mock<IClientProxy>();
			var authClient = new TestAuthorizationClient(new[] { "1", "2" });

			clients.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxy.Object);
			hubContext.Setup(h => h.Clients).Returns(clients.Object);
			clientProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			var controller = CreateController(db, hubContext, authClient, "1");

			SendRequest request = new SendRequest
			{
				RecipientId = "2",
				Text = "Hello Persistence"
			};


			var result = await controller.Send(request);

			var messages = await db.Messages.ToListAsync();
			Assert.Single(messages);

			var saved = messages[0];
			Assert.Equal("1", saved.SenderId);
			Assert.Equal("2", saved.RecipientId);
			Assert.Equal("Hello Persistence", saved.Text);
			Assert.InRange((DateTime.UtcNow - saved.SentAt).TotalSeconds, 0, 5);

			Assert.IsType<OkResult>(result);
		}

		[Fact]
		public async Task Send_Should_SendMessageViaSignalR_WithCorrectPayload()
		{
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("SignalRTestDb")
				.Options;
			await using var db = new MessageDbContext(options);

			var hubContext = new Mock<IHubContext<MessagesHub>>();
			var clients = new Mock<IHubClients>();
			var clientProxy = new Mock<IClientProxy>();
			var authClient = new TestAuthorizationClient(new[] { "1", "2" });

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

			// Controller sends to Group(userId) where userId comes from claims (sender).
			clients.Setup(c => c.Group("1")).Returns(clientProxy.Object);
			hubContext.Setup(h => h.Clients).Returns(clients.Object);

			var controller = CreateController(db, hubContext, authClient, "1");

			SendRequest request = new SendRequest
			{
				RecipientId = "2",
				Text = "Hello Test"
			};


			var result = await controller.Send(request);

			clients.Verify(c => c.Group("1"), Times.Once);
			clientProxy.Verify(p =>
				p.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
				Times.Once);

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

			Assert.IsType<OkResult>(result);
		}

		[Fact]
		public async Task Send_Should_Return_BadRequest_On_ValidationFailure()
		{
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("testdb-badrequest")
				.Options;

			await using var db = new MessageDbContext(options);

			var hubContext = new Mock<IHubContext<MessagesHub>>();
			var clients = new Mock<IHubClients>();
			var clientProxy = new Mock<IClientProxy>();
			// Add both "1" (sender) and "2" (recipient) as valid users
			var authClient = new TestAuthorizationClient(new[] { "1", "2" });

			clients.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxy.Object);
			hubContext.Setup(h => h.Clients).Returns(clients.Object);
			clientProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			var controller = CreateController(db, hubContext, authClient, "1");

			SendRequest request1 = new SendRequest
			{
				RecipientId = "",
				Text = ""
			};

			SendRequest request2 = new SendRequest
			{
				RecipientId = "2",
				Text = ""
			};

			SendRequest request3 = new SendRequest
			{
				RecipientId = "",
				Text = "test"
			};

			// Only test for missing/empty recipientId or text, but with a valid sender and recipient
			var result1 = await controller.Send(request1);         // Both empty
			var result2 = await controller.Send(request2);        // Empty text
			var result3 = await controller.Send(request3);     // Empty recipient

			Assert.IsType<BadRequestObjectResult>(result1);
			Assert.IsType<BadRequestObjectResult>(result2);
			Assert.IsType<BadRequestObjectResult>(result3);
		}

		[Fact]
		public async Task Send_Should_Return_NotFound_If_Sender_Or_Recipient_Not_Found()
		{
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("testdb-notfound")
				.Options;

			await using var db = new MessageDbContext(options);

			var hubContext = new Mock<IHubContext<MessagesHub>>();
			var clients = new Mock<IHubClients>();
			var clientProxy = new Mock<IClientProxy>();

			clients.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxy.Object);
			hubContext.Setup(h => h.Clients).Returns(clients.Object);
			clientProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			// Sender not found
			var authClient1 = new TestAuthorizationClient(new[] { "3" });
			var controller1 = CreateController(db, hubContext, authClient1, "1");
			SendRequest request1 = new SendRequest
			{
				RecipientId = "2",
				Text = "test"
			};
			var result1 = await controller1.Send(request1);
			Assert.IsType<NotFoundObjectResult>(result1);

			// Recipient not found
			var authClient2 = new TestAuthorizationClient(new[] { "1" });
			var controller2 = CreateController(db, hubContext, authClient2, "1");
			SendRequest request2 = new SendRequest
			{
				RecipientId = "3",
				Text = "test"
			};
			var result2 = await controller2.Send(request2);
			Assert.IsType<NotFoundObjectResult>(result2);
		}
	}
}