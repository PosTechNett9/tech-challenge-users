using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace FIAP.CloudGames.Users.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var activity = Activity.Current;

            Log.Information(
                "Login request received | TraceId: {TraceId}",
                activity?.TraceId.ToString()
            );

            try
            {
                var token = await _authService.LoginAsync(dto);

                Log.Information(
                    "Login successful | TraceId: {TraceId}",
                    activity?.TraceId.ToString()
                );

                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(
                    ex,
                    "Unauthorized login attempt | TraceId: {TraceId}",
                    activity?.TraceId.ToString()
                );

                return Unauthorized(new { ex.Message });
            }
        }
    }
}
