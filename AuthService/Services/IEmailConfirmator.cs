namespace AuthService.Services
{
	public interface IEmailConfirmator
	{
		Task SendConfirmationEmailAsync(ApplicationUser user, string confirmationLink);
	}
}
