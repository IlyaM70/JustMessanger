using MessageService.Data;
using MessageService.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MessageService.Models;
using MessageService.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
		#region ctor
		private readonly MessageDbContext _db;
        private readonly IHubContext<MessagesHub> _hub;

        public MessageController(MessageDbContext db, IHubContext<MessagesHub> hub)
        {
            _db = db;
            _hub = hub;
        }
		#endregion

		#region send
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

            // 2) Push over SignalR only to the recipient
            await _hub.Clients
                .Group(dto.RecipientId!) // sends only to the connections in that group
                .SendAsync("ReceiveMessage", new
                {
                    msg.Id,
                    msg.SenderId,
                    msg.Text,
                    msg.SentAt
                });

            return Ok();
        }
		#endregion

		#region history
		[HttpGet("history")]
		public async Task<IActionResult> GetHistory([FromQuery] string userId, [FromQuery] string otherUserId)
		{
			var messages = await _db.Messages
				.Where(m =>
					(m.SenderId == userId && m.RecipientId == otherUserId) ||
					(m.SenderId == otherUserId && m.RecipientId == userId))
				.OrderBy(m => m.SentAt)
				.ToListAsync();

			return Ok(messages.Select(m => new {
				m.Id,
				m.SenderId,
				m.RecipientId,
				m.Text,
				m.SentAt
			}));
		}
		#endregion
	}


}
