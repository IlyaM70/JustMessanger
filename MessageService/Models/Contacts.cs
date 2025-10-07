using System.ComponentModel.DataAnnotations;

namespace MessageService.Models
{
	public class Contact
	{		
		public string UserId { get; set; } = "";
		public string Email { get; set; } = "";
		public string UserName { get; set; } = "";
		public string LastMessage { get; set; } = "";
		public DateTime LastMessageAt { get; set; } = DateTime.MinValue;
	}
}
