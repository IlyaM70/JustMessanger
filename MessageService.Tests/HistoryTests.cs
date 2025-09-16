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
        private MessageController CreateController(MessageDbContext db)
        {
            var hubContext = new Mock<IHubContext<MessagesHub>>();
            // GetHistory does not use AuthorizationClient, pass a simple instance
            var authClient = new MessageService.AuthorizationClient(new HttpClient());
            return new MessageController(db, hubContext.Object, authClient);
        }

        [Fact]
        public async Task GetHistory_Should_Return_BadRequest_When_UserIds_Empty()
        {
            var options = new DbContextOptionsBuilder<MessageDbContext>()
                .UseInMemoryDatabase("History_BadRequest_Db")
                .Options;

            await using var db = new MessageDbContext(options);
            var controller = CreateController(db);

            var res1 = await controller.GetHistory("", "2");
            var res2 = await controller.GetHistory("1", "");

            Assert.IsType<BadRequestObjectResult>(res1);
            Assert.IsType<BadRequestObjectResult>(res2);
        }

        [Fact]
        public async Task GetHistory_Should_Return_Empty_List_When_No_Messages()
        {
            var options = new DbContextOptionsBuilder<MessageDbContext>()
                .UseInMemoryDatabase("History_Empty_Db")
                .Options;

            await using var db = new MessageDbContext(options);
            var controller = CreateController(db);

            var res = await controller.GetHistory("1", "2");

            var ok = Assert.IsType<OkObjectResult>(res);
            var list = Assert.IsType<List<Message>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetHistory_Should_Return_Only_Messages_Between_Users_In_Order()
        {
            var options = new DbContextOptionsBuilder<MessageDbContext>()
                .UseInMemoryDatabase("History_Messages_Db")
                .Options;

            await using var db = new MessageDbContext(options);

            // messages between 1 and 2
            var m1 = new Message { SenderId = "1", RecipientId = "2", Text = "first", SentAt = DateTime.UtcNow.AddMinutes(-10) };
            var m2 = new Message { SenderId = "2", RecipientId = "1", Text = "second", SentAt = DateTime.UtcNow.AddMinutes(-5) };
            var m3 = new Message { SenderId = "1", RecipientId = "2", Text = "third", SentAt = DateTime.UtcNow.AddMinutes(-1) };

            // unrelated messages
            var other1 = new Message { SenderId = "3", RecipientId = "1", Text = "other", SentAt = DateTime.UtcNow.AddMinutes(-2) };

            await db.Messages.AddRangeAsync(m1, m2, m3, other1);
            await db.SaveChangesAsync();

            var controller = CreateController(db);

            var res = await controller.GetHistory("1", "2");

            var ok = Assert.IsType<OkObjectResult>(res);
            var list = Assert.IsType<List<Message>>(ok.Value);

            // should only contain m1,m2,m3 in chronological order
            Assert.Equal(3, list.Count);
            Assert.Equal("first", list[0].Text);
            Assert.Equal("second", list[1].Text);
            Assert.Equal("third", list[2].Text);

            // ensure none of the returned messages involve other users
            Assert.All(list, msg => Assert.True((msg.SenderId == "1" && msg.RecipientId == "2") || (msg.SenderId == "2" && msg.RecipientId == "1")));
        }
    }
}
