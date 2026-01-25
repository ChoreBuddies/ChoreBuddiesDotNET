using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authentication;
using Shared.Users;

namespace ChoreBuddies.Backend.Features.Users;

[ApiController]
[Route("/api/v1/users")]
[Authorize]
public class AppUserController(
    IAppUserService userService,
    ITokenService tokenService,
    IMapper mapper) : ControllerBase
{
    private readonly IAppUserService _userService = userService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IMapper _mapper = mapper;

    private async Task<AppUser?> GetCurrentUser()
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        return await _userService.GetUserByIdAsync(userId);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserAsync(int userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return BadRequest();

        return Ok(_mapper.Map<AppUserDto>(user));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync()
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();

        return Ok(_mapper.Map<AppUserDto>(user));
    }

    [HttpGet("myPoints")]
    public async Task<IActionResult> GetMyPointsAsync()
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var result = await _userService.GetUserPointsCountAsync(userId);

        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMeAsync([FromBody] UpdateAppUserDto updateAppUserDto)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var result = await _userService.UpdateUserAsync(userId, updateAppUserDto);

        return Ok(result);
    }

    [HttpPut("{userId}")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<IActionResult> UpdateUserAsync(int userId, [FromBody] UpdateAppUserDto updateAppUserDto)
    {
        var result = await _userService.UpdateUserAsync(userId, updateAppUserDto);

        return Ok(result);
    }
    [HttpPut("fcmtoken")]
    public async Task<IActionResult> UpdateFcmTokenAsync([FromBody] UpdateFcmTokenDto updateFcmTokenDto)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var result = await _userService.UpdateFcmTokenAsync(userId, updateFcmTokenDto);

        return Ok(result);
    }

    [HttpGet("household")]
    public async Task<IActionResult> GetMyHouseholdMembers([FromQuery] bool? role = false)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        if (role ?? false)
        {
            return Ok(await _userService.GetUsersHouseholdMembersWithRolesAsync(userId));
        }
        var result = await _userService.GetUsersHouseholdMembersAsync(userId);

        var resultDto = result.Select(v => _mapper.Map<AppUserMinimalDto>(v));

        return Ok(resultDto);
    }

    [HttpGet("household/adults")]
    public async Task<IActionResult> GetUsersHouseholdAdults()
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();
        var adults = await _userService.GetUsersHouseholdAdultsAsync(user.Id);
        var resultDto = adults.Select(v => _mapper.Map<AppUserDto>(v));
        return Ok(resultDto);
    }

    [HttpGet("household/children")]
    public async Task<IActionResult> GetUsersHouseholdChildrenAsync()
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();
        var children = await _userService.GetUsersHouseholdChildrensAsync(user.Id);
        var resultDto = children.Select(v => _mapper.Map<AppUserDto>(v));
        return Ok(resultDto);
    }

    [HttpPut("role")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<IActionResult> UpdateUserRoleAsync([FromBody] UpdateRoleDto dto)
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(dto.RoleName))
            return BadRequest("RoleName is required.");
        try
        {
            var success = await _userService.UpdateUserRoleAsync(dto.Id, dto.RoleName);
            if (!success)
                return StatusCode(500, "Failed to update role.");

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("role")]
    public async Task<IActionResult> GetAvailableRolesAsync()
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();
        var result = await _userService.GetAvailableRolesAsync();
        return Ok(result);
    }

}
