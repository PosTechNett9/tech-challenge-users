using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FIAP.CloudGames.Users.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController: ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly ActivitySource _activitySource;

        public UserController(IUserService userService,
        ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
            _activitySource = new ActivitySource("UsersService");
        }
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAll()
        {
            var id = Guid.NewGuid();
            using var activity = _activitySource.StartActivity("GetUser");
            activity?.SetTag("user.id", id);

            _logger.LogInformation("Buscando usuários {id}", id);

            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetById(Guid id)
        {
            using var activity = _activitySource.StartActivity("GetUser");
            activity?.SetTag("user.id", id);

            _logger.LogInformation(
                "GetById solicitado | UserId: {UserId} | TraceId: {TraceId}",
                id,
                activity.TraceId
            );

            var user = await _userService.GetByIdAsync(id);

            if (user is null)
            {
                _logger.LogWarning(
                    "User não encontrado | UserId: {UserId} | TraceId: {TraceId}",
                    id,
                    activity.TraceId
                );

                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var traceId = Activity.Current?.TraceId.ToString();

            _logger.LogInformation(
                "Create user solicitado | Email: {Email} | TraceId: {TraceId}",
                dto.Email,
                traceId
            );

            var createdUser = await _userService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdUser.Id },
                createdUser
            );
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
        {
            var traceId = Activity.Current?.TraceId.ToString();

            _logger.LogInformation(
                "Update user solicitado | UserId: {UserId} | TraceId: {TraceId}",
                dto.Id,
                traceId
            );

            var updatedUser = await _userService.UpdateAsync(dto);

            if (updatedUser is null)
            {
                _logger.LogWarning(
                    "Update falhou, user não encontrado | UserId: {UserId} | TraceId: {TraceId}",
                    dto.Id,
                    traceId
                );

                return NotFound();
            }

            return Ok(updatedUser);
        }

        [HttpPut("update-password")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserPasswordDto dto)
        {
            var traceId = Activity.Current?.TraceId.ToString();

            _logger.LogInformation(
                "Update password solicitado | UserId: {UserId} | TraceId: {TraceId}",
                dto.Id,
                traceId
            );

            var updatedUser = await _userService.UpdatePasswordAsync(dto);

            if (updatedUser is null)
            {
                _logger.LogWarning(
                    "Update password falhou, user não encontrado | UserId: {UserId} | TraceId: {TraceId}",
                    dto.Id,
                    traceId
                );

                return NotFound();
            }

            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var traceId = Activity.Current?.TraceId.ToString();

            _logger.LogInformation(
                "Delete user solicitado | UserId: {UserId} | TraceId: {TraceId}",
                id,
                traceId
            );

            var deletedUser = await _userService.DeleteAsync(id);

            if (!deletedUser)
            {
                _logger.LogWarning(
                    "Delete falhou, user não encontrado | UserId: {UserId} | TraceId: {TraceId}",
                    id,
                    traceId
                );

                return NotFound();
            }

            _logger.LogInformation(
                "User deletado com sucesso | UserId: {UserId} | TraceId: {TraceId}",
                id,
                traceId
            );

            return NoContent();
        }
    }
}
