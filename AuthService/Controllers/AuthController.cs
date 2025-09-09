using AuthService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		#region ctor
		private readonly AuthDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		public AuthController(AuthDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}
		#endregion

		//Register
		[HttpPost("register")]
		public IActionResult Register(string username,string email, string password)
		{
			var existingUser = _db.Users.FirstOrDefault(u => u.Email == email);
			if (existingUser != null)
			{
				return BadRequest("Email already in use.");
			}

			var result = _userManager.CreateAsync(new ApplicationUser
			{
				UserName = username,
				Email = email,
			}, password).Result;

			if (!result.Succeeded)
			{
				string errors = string.Empty;

				foreach (var error in result.Errors)
				{
					errors += $"Error: {error.Code} - {error.Description}\n";
				}

				return BadRequest(errors);
			}

			//To do: send confirmation email

			return Ok("You successfully registered!");
		}

		//confirm email
		//To do

		//Login
		//To do

		//Logout
		//to do
	}
}
