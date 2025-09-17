namespace AuthService.Services
{
	public interface IEmailConfirmator
	{
		public Task SendConfirmationEmailAsync(ApplicationUser user, string token, string baseUrl);
	}
}
