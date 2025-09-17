using Microsoft.AspNetCore.Identity;

namespace AuthService.Services
{
	public class EmailConfirmator : IEmailConfirmator
	{

		public async Task SendConfirmationEmailAsync(ApplicationUser user, string token, string baseUrl)
		{
			var confirmationLink = $"{baseUrl}/api/auth/confirmemail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
			Console.WriteLine($"Confirm link: {confirmationLink}");
		}

	}
}
