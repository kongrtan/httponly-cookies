using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlazorWasmCookieAuth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BlazorWasmCookieAuth.Api.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase {
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger) {
			_logger = logger;
		}
		[HttpGet("check")]
		public IActionResult CheckTokenFromCookie() {
			// 쿠키에서 access_token 가져오기
			if (Request.Cookies.TryGetValue("access_token", out var token)) {
				// 토큰이 존재할 경우 처리
				return Ok(new { message = "토큰 있음", token });
			}

			return Unauthorized(new { message = "토큰 없음" });
		}


		[HttpGet]
		public IEnumerable<WeatherForecast> GetPublic() {
			return CreateData();
		}

		[HttpGet("protected")]
		[Authorize]
		public IEnumerable<WeatherForecast> GetProtected() {
			return CreateData();
		}

		private IEnumerable<WeatherForecast> CreateData() {
			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
				Date = DateTime.Now.AddDays(index),
				TemperatureC = rng.Next(-20, 55),
				Summary = Summaries[rng.Next(Summaries.Length)]
			})
			.ToArray();
		}


		[HttpGet("login")]
		public IActionResult Login() {
			// 여기서는 예시로 간단히 로그인 검증 (실제로는 DB 검사 등 필요)

			var token = GenerateJwtToken("hyunsoft");

			var cookieOptions = new CookieOptions {
				HttpOnly = true,
				//Secure = true, // HTTPS에서만 전송됨
				SameSite = SameSiteMode.Lax,
				Expires = DateTimeOffset.UtcNow.AddHours(1)
			};

			Response.Cookies.Append("access_token", token, cookieOptions);

			return Ok(new { message = "로그인 성공" });

		}

		private string GenerateJwtToken(string username) {
			// JWT 생성 로직 (간단한 예시)
			var handler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes("your-super-long-secret-key-1234567890");

			var descriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new[]
				{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.Role, "admin"),
			}),
				Expires = DateTime.UtcNow.AddHours(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = handler.CreateToken(descriptor);
			return handler.WriteToken(token);
		}
	}
}
