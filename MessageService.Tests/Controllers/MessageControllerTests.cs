using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MessageService.Controllers;
using MessageService.Data;
using MessageService.Models;
using MessageService.Models.Dtos;
using MessageService.Hubs;
using Microsoft.AspNetCore.Mvc;

public class MessageControllerTests
{
	[Fact]
	public async Task Send_Should_AddMessageToDatabase()
	{
		// 1) Arrange: create in‑memory DbContext
		var options = new DbContextOptionsBuilder<MessageDbContext>()
			.UseInMemoryDatabase("PersistTestDb")
			.Options;
		await using var db = new MessageDbContext(options);

		// 2) Arrange: mock IHubContext but ignore calls
		var hubContextMock = new Mock<IHubContext<MessagesHub>>();

		var mockHubContext = new Mock<IHubContext<MessagesHub>>();
		var mockClients = new Mock<IHubClients>();
		var mockClientProxy = new Mock<IClientProxy>();

		mockClients
			.Setup(c => c.Group(It.IsAny<string>()))
			.Returns(mockClientProxy.Object);

		mockHubContext
			.Setup(h => h.Clients)
			.Returns(mockClients.Object);

		mockClientProxy
			.Setup(p => p.SendAsync(
				It.IsAny<string>(),
				It.IsAny<object[]>(),
				It.IsAny<CancellationToken>())
			)
			.Returns(Task.CompletedTask);



		// 3) Act: call Send()
		var controller = new MessageController(db, hubContextMock.Object);

		var dto = new SendMessageDto
		{
			SenderId = "1",
			RecipientId = "2",
			Text = "Hello Persistence"
		};

		var result = await controller.Send(dto);

		// 4) Assert: method returned OkResult
		Assert.IsType<OkResult>(result);

		// 5) Assert: database contains exactly one message
		var messages = await db.Messages.ToListAsync();
		Assert.Single(messages);

		var saved = messages[0];
		Assert.Equal("1", saved.SenderId);
		Assert.Equal("2", saved.RecipientId);
		Assert.Equal("Hello Persistence", saved.Text);

		// 6) Assert: SentAt is recent (within last 5 seconds)
		Assert.InRange((DateTime.UtcNow - saved.SentAt).TotalSeconds, 0, 5);
	}

}
