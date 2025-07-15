using MessageService.Controllers;
using MessageService.Data;
using MessageService.Hubs;
using MessageService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MessageService.Tests.Controllers.Messages
{
	public class HistoryTests
	{
		#region History_SuccesShould_ReturnOkAndMessagesBetweenTwoUsersInCorrectOrder
		[Fact]
		public async Task History_SuccesShould_ReturnOkAndMessagesBetweenTwoUsersInCorrectOrder()
		{
			#region Arrange
			//Arrange: create in‑memory DbContext
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("testdb")
				.Options;
			await using var db = new MessageDbContext(options);

			db.Messages.AddRange(
			  new Message { SenderId = "A", RecipientId = "B", Text = "A→B", SentAt = DateTime.Now.AddMinutes(1) },
			  new Message { SenderId = "B", RecipientId = "A", Text = "B→A", SentAt = DateTime.Now.AddMinutes(2) },
			  new Message { SenderId = "A", RecipientId = "B", Text = "A→B(2)", SentAt = DateTime.Now.AddMinutes(3) },
			  new Message { SenderId = "B", RecipientId = "A", Text = "B→A(2)", SentAt = DateTime.Now.AddMinutes(4) },
			  // “noise” messages
			  new Message { SenderId = "A", RecipientId = "C", Text = "A→C", SentAt = DateTime.Now.AddMinutes(1)},
			  new Message { SenderId = "C", RecipientId = "A", Text = "C→A", SentAt = DateTime.Now.AddMinutes(2)},
			  new Message { SenderId = "A", RecipientId = "C", Text = "A→C(2)", SentAt = DateTime.Now.AddMinutes(3)},
			  new Message { SenderId = "C", RecipientId = "A", Text = "A→C (2)", SentAt = DateTime.Now.AddMinutes(4)}
			);
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


			//Arrange controller
			var controller = new MessageController(db, hubContext.Object);

			#endregion

			//Act: call hystory()
			var result = await controller.GetHistory("A","B");

			// Assert 1 & 2: Ensure it’s an OkObjectResult
			var okResult = Assert.IsType<OkObjectResult>(result);

			// Assert 3: Ensure the payload is an enumerable
			var payload = Assert.IsAssignableFrom<System.Collections.IEnumerable>(okResult.Value)
				.Cast<object>()
				.ToList();

			// Assert 4: Only the 4 A↔B messages are returned
			Assert.Equal(4, payload.Count);

			// Helper to extract a property by name
			T GetProp<T>(object obj, string propName) =>
				(T)obj.GetType().GetProperty(propName)!.GetValue(obj)!;

			// Assert 5: Check each item’s details and ordering
			var m1 = payload[0];
			Assert.Equal("A", GetProp<string>(m1, "SenderId"));
			Assert.Equal("B", GetProp<string>(m1, "RecipientId"));
			Assert.Equal("A→B", GetProp<string>(m1, "Text"));

			var m2 = payload[1];
			Assert.Equal("B", GetProp<string>(m2, "SenderId"));
			Assert.Equal("A", GetProp<string>(m2, "RecipientId"));
			Assert.Equal("B→A", GetProp<string>(m2, "Text"));

			var m3 = payload[2];
			Assert.Equal("A", GetProp<string>(m3, "SenderId"));
			Assert.Equal("B", GetProp<string>(m3, "RecipientId"));
			Assert.Equal("A→B(2)", GetProp<string>(m3, "Text"));

			var m4 = payload[3];
			Assert.Equal("B", GetProp<string>(m4, "SenderId"));
			Assert.Equal("A", GetProp<string>(m4, "RecipientId"));
			Assert.Equal("B→A(2)", GetProp<string>(m4, "Text"));

		}
		#endregion

		#region History_Should_Return_BadRequest_On_ValidationFailure
		[Fact]
		public async Task History_Should_Return_BadRequest_On_ValidationFailure()
		{
			#region Arrange
			var options = new DbContextOptionsBuilder<MessageDbContext>()
				.UseInMemoryDatabase("testdb-history-badrequest")
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

			db.Messages.AddRange(
				  new Message { SenderId = "A", RecipientId = "B", Text = "A→B", SentAt = DateTime.Now.AddMinutes(1) },
				  new Message { SenderId = "B", RecipientId = "A", Text = "B→A", SentAt = DateTime.Now.AddMinutes(2) },
				  new Message { SenderId = "A", RecipientId = "B", Text = "A→B(2)", SentAt = DateTime.Now.AddMinutes(3) },
				  new Message { SenderId = "B", RecipientId = "A", Text = "B→A(2)", SentAt = DateTime.Now.AddMinutes(4) },
				  // “noise” messages
				  new Message { SenderId = "A", RecipientId = "C", Text = "A→C", SentAt = DateTime.Now.AddMinutes(1) },
				  new Message { SenderId = "C", RecipientId = "A", Text = "C→A", SentAt = DateTime.Now.AddMinutes(2) },
				  new Message { SenderId = "A", RecipientId = "C", Text = "A→C(2)", SentAt = DateTime.Now.AddMinutes(3) },
				  new Message { SenderId = "C", RecipientId = "A", Text = "A→C (2)", SentAt = DateTime.Now.AddMinutes(4) }
				);
			#endregion

			// Act
			var result = await controller.GetHistory("", "B");
			var result2 = await controller.GetHistory("A", "");
			var result3 = await controller.GetHistory("", "");


			// Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.IsType<BadRequestObjectResult>(result2);
			Assert.IsType<BadRequestObjectResult>(result3);
		}
		#endregion
	}
}
