namespace MessageService
{
	public class AuthorizationClient
	{
		private readonly HttpClient _httpClient;

		public  AuthorizationClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public virtual async Task<bool> IsUserExistAsync(string userId)
		{
			var response = await _httpClient.GetAsync($"/api/auth/userexist/{userId}");
			return response.IsSuccessStatusCode;
		}
	}
}
