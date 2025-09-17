using Microsoft.AspNetCore.Identity;

namespace AuthService.Services
{
	public class EmailConfirmator : IEmailConfirmator
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public EmailConfirmator(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task SendConfirmationEmailAsync(ApplicationUser user, string confirmationLink)
		{
			// TODO: actually send via SMTP, SendGrid, etc.
			Console.WriteLine($"Confirm link for {user.Email}: {confirmationLink}");
		}
	}
}
