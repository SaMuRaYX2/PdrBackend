using Microsoft.AspNetCore.Authorization;

namespace TrafficRules.Api.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrafficRules.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly string _googleClientId;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _googleClientId = config["Authentication:Google:ClientId"];
    }
    
    public class GoogleLoginRequest
    {
        public string Token { get; set; }
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _googleClientId }
            });

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest("Не вдалося створити користувача!!!");
                }
                
            }
            await _signInManager.SignInAsync(user, isPersistent: true);
            return Ok(new { message = "Успішний вхід через Google!"});
        }
        catch (InvalidJwtException ex)
        {
            Console.WriteLine($"Google token validation failed: {ex.Message}");
            return Unauthorized($"Недійсний Google токен: {ex.Message}");
        }
    }
    [Authorize( Roles = "Admin")]
    [HttpPost("make-me-admin")]
    public async Task<IActionResult> MakeMeAdmin([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound("User is not found");

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        
        return Ok(new {message = "Вам надано права адміністратора. Перезайдіть у систему."});
    }

    [HttpGet("is-admin")]
    public IActionResult IsAdmin()
    {
        bool isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
        return Ok(new { isAdmin });
    }
}