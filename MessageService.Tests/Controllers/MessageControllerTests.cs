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

		//Assert: method returned OkResult
		Assert.IsType<OkResult>(result);

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

}
