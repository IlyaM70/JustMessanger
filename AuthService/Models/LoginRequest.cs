using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
	public class LoginRequest
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
