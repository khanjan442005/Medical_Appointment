using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers.Api;

[ApiController]
[Route("api/auth")]
public sealed class AuthApiController : ControllerBase
{
    private readonly IDemoPortalService _portalService;

    public AuthApiController(IDemoPortalService portalService)
    {
        _portalService = portalService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginApiRequest request)
    {
        var (success, errorMessage) = await _portalService.TryLoginAsync(request.Role, request.Email, request.Password);
        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Login successful.", role = request.Role });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _portalService.LogoutAsync();
        return Ok(new { message = "Logout successful." });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordApiRequest request)
    {
        if (!_portalService.TryBeginPasswordReset(request.Email, out var normalizedEmail, out var errorMessage))
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Email verified.", email = normalizedEmail });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordApiRequest request)
    {
        if (!_portalService.ResetPassword(request.Email, request.Password, out var errorMessage))
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Password updated successfully." });
    }

    public sealed class LoginApiRequest
    {
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class ForgotPasswordApiRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordApiRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
