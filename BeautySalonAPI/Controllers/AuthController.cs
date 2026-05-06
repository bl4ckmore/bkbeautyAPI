using BeautySalonAPI.DTOs.Auth;
using BeautySalonAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BeautySalonAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(result);
    }
}
