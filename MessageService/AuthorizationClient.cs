using MessageService.Models;
using Microsoft.AspNetCore.Mvc;

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

		public virtual async Task<IActionResult> FillContacts(List<Contact> contacts)
		{
			var response = await _httpClient.PostAsJsonAsync("/api/auth/fillcontacts",contacts);
			if (response.IsSuccessStatusCode)
			{
				var users = await response.Content.ReadFromJsonAsync<IEnumerable<object>>();
				return new OkObjectResult(users);
			}
			else
			{
				return new StatusCodeResult((int)response.StatusCode);
			}
		}
	}
}
