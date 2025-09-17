using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace AuthService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		#region ctor
		private readonly AuthDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _configuration;
		private readonly IEmailConfirmator _emailConfirmator;
		public AuthController(AuthDbContext db, UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailConfirmator emailConfirmator)
		{
			_db = db;
			_userManager = userManager;
			_configuration = configuration;
			_emailConfirmator = emailConfirmator;
		}
		#endregion

		#region Register
		[HttpPost("register")]
		public async Task<IActionResult> Register(string username,string email, string password)
		{
			var existingUser = _db.Users.FirstOrDefault(u => u.Email == email);
			if (existingUser != null)
			{
				return BadRequest("Email already in use.");
			}

			ApplicationUser user = new ApplicationUser
			{
				UserName = username,
				Email = email,
			};

			var result = _userManager.CreateAsync(user, password).Result;

			if (!result.Succeeded)
			{
				string errors = string.Empty;

				foreach (var error in result.Errors)
				{
					errors += $"Error: {error.Code} - {error.Description}\n";
				}

				return BadRequest(errors);
			}

			// Generate confirmation token
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			// Build callback URL (this stays in controller, where HttpContext exists)
			var confirmationLink = Url.Action(
				nameof(ConfirmEmail),
				"Auth",
				new { userId = user.Id, token = token },
				protocol: HttpContext.Request.Scheme);

			// Delegate actual email sending to the service
			await _emailConfirmator.SendConfirmationEmailAsync(user, confirmationLink);

			return Ok("You successfully registered! Please confirm your email.");
		}
		#endregion

		#region confirmemail
		[HttpGet("confirmemail")]
		public IActionResult ConfirmEmail (string userId, string token)
		{
			if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
			{
				return BadRequest("User ID and Token are required");
			}

			var user = _db.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null)
			{
				return BadRequest("User was not found");
			}
			var result = _userManager.ConfirmEmailAsync(user, token).Result;

			if (!result.Succeeded)
			{
				string errors = string.Empty;

				foreach (var error in result.Errors)
				{
					errors += $"Error: {error.Code} - {error.Description}\n";
				}

				return BadRequest(errors);
			}

			return Ok("Email confirmed successfully!");
		}
		#endregion

		#region Login
		[HttpPost("login")]
		public  IActionResult Login(string email,string password)
		{
			var user = _db.Users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				return BadRequest("Invalid email or password.");
			}
			bool passwordValid = _userManager.CheckPasswordAsync(user, password).Result;
			if (!passwordValid)
			{
				return BadRequest("Invalid email or password.");
			}

			//Issie a JWT
			string token = GenerateToken(user);


			return Ok(new {token});
		}
		#endregion

		#region GenerateToken
		private string GenerateToken(ApplicationUser user)
		{
			//claims
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim("uid", user.Id)
			};

			//key+credentials
			var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			//token
			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddDays(7),
				signingCredentials: creds
				);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
		#endregion

		#region UserExist
		[HttpGet("userexist/{userId}")]
		public IActionResult UserExist(string userId)
		{
			var user = _db.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null)
			{
				return NotFound("User was not found");
			}
			return Ok("User found");
		}
		#endregion

	}
}
