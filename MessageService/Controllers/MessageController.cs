using MessageService.Data;
using MessageService.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MessageService.Models;
using System.ComponentModel.DataAnnotations;
using MessageService.Models.Dtos;

namespace MessageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly MessageDbContext _db;
        private readonly IHubContext<MessagesHub> _hub;

        public MessageController(MessageDbContext db, IHubContext<MessagesHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
        {
            // 1) Persist
            Message msg = new Message()
            {
                SenderId = dto.SenderId,
                RecipientId = dto.RecipientId,
                Text = dto.Text,
                SentAt = DateTime.UtcNow
            };
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();

            // 2) Push over SignalR
            await _hub.Clients
                      .User(dto.RecipientId)
                      .SendAsync("ReceiveMessage", new
                      {
                          msg.Id,
                          msg.SenderId,
                          msg.Text,
                          msg.SentAt
                      });

            return Ok();
        }
    }


}
