using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Users;
using System.Security.Claims;

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

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMeAsync([FromBody] UpdateAppUserDto updateAppUserDto)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var result = await _userService.UpdateUserAsync(userId, updateAppUserDto);

        return Ok(result);
    }

    [HttpPut("{userId}")]
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
    public async Task<IActionResult> GetMyHouseholdMembers()
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var result = await _userService.GetUsersHouseholdMembersAsync(userId);

        var resultDto = result.Select(v => _mapper.Map<AppUserDto>(v));

        return Ok(resultDto);
    }

}
