using MessageService.Data;
using MessageService.Hubs;
using MessageService.Models;
using MessageService.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	[Authorize]
	public class MessageController : ControllerBase
    {
		#region ctor
		private readonly MessageDbContext _db;
        private readonly IHubContext<MessagesHub> _hub;
		private readonly AuthorizationClient _authClient;

		public MessageController(MessageDbContext db, IHubContext<MessagesHub> hub,
			AuthorizationClient authClient)
        {
            _db = db;
            _hub = hub;
			_authClient = authClient;
		}
		#endregion

		#region send
		[HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
        {
			# region Validate
			if (string.IsNullOrEmpty(dto.SenderId))
            {
                return BadRequest("ERROR: SenderId is empty");
            }

			if (string.IsNullOrEmpty(dto.RecipientId))
			{
				return BadRequest("ERROR: RecipientId is empty");
			}

			if (string.IsNullOrEmpty(dto.Text))
			{
				return BadRequest("ERROR: Text of message is empty");
			}

			if (!ModelState.IsValid)
			{
				IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(allErrors);
			}

			if (!await _authClient.IsUserExistAsync(dto.SenderId))
			{
				return NotFound("ERROR: Sender with given ID was not found in the database");
			}

			if (!await _authClient.IsUserExistAsync(dto.RecipientId))
			{
				return NotFound("ERROR: Recipient with given ID was not found in the database");
			}

			#endregion

			//Persist
			Message msg = new Message()
            {
                SenderId = dto.SenderId,
                RecipientId = dto.RecipientId,
                Text = dto.Text,
                SentAt = DateTime.UtcNow
            };
            _db.Messages.Add(msg);

			//Save changes
			try
			{
				await _db.SaveChangesAsync();

			}
			catch (DbUpdateException dbUpdateEx)
			{
				return StatusCode(500, $"ERROR: Database update failed: {dbUpdateEx.Message}");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Unexpected ERROR: {ex.Message}");
			}

			//Push over SignalR only to the recipient
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
			//Validate
			if(string.IsNullOrEmpty(userId))
			{
				return BadRequest("ERROR: UserId is empty");
			}
			if (string.IsNullOrEmpty(otherUserId))
			{
				return BadRequest("ERROR: OtherUserId is empty");
			}

			List<Message>? messages = [];
			try
			{
			messages = await _db.Messages
				.Where(m =>
					(m.SenderId == userId && m.RecipientId == otherUserId) ||
					(m.SenderId == otherUserId && m.RecipientId == userId))
				.OrderBy(m => m.SentAt)
				.ToListAsync();
			}
			catch (InvalidOperationException ex)
			{
				return StatusCode(500, $"Database operation error: {ex.Message}");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Unexpected error: {ex.Message}");
			}

			return Ok(messages);
		}
		#endregion
	}


}
