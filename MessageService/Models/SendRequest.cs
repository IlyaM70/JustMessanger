using System.ComponentModel.DataAnnotations;

namespace MessageService.Models
{
	public class SendRequest
	{
		[Required]
		public string RecipientId { get; set; }
		[Required]
		public string Text { get; set; }
	}
}
