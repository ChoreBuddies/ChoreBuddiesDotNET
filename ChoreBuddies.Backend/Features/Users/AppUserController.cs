using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Users;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Users;

[ApiController]
[Route("/api/v1/users")]
[Authorize]
public class AppUserController(IAppUserService userService, IMapper mapper) : ControllerBase
{
    private readonly IAppUserService _userService = userService;
    private readonly IMapper _mapper = mapper;

    private async Task<AppUser?> GetCurrentUser()
    {
        var emailJWT = User.FindFirst(ClaimTypes.Email)?.Value;
        if (emailJWT == null)
            return null;
        return await _userService.GetUserByEmailAsync(emailJWT);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserAsync(int userId)
    {
        //var emailJWT = User.FindFirst(ClaimTypes.Email)?.Value;

        var user = await _userService.GetUserByIdAsync(userId);
        //if (user == null || string.IsNullOrEmpty(emailJWT) || user.Email != emailJWT)
        //    return BadRequest();

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
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();

        var result = await _userService.UpdateUserAsync(updateAppUserDto.Id, updateAppUserDto);

        return Ok(result);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUserAsync(int userId, [FromBody] UpdateAppUserDto updateAppUserDto)
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();

        var result = await _userService.UpdateUserAsync(userId, updateAppUserDto);

        return Ok(result);
    }
    [HttpPut("fcmtoken")]
    public async Task<IActionResult> UpdateFcmTokenAsync([FromBody] UpdateFcmTokenDto updateFcmTokenDto)
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();

        var result = await _userService.UpdateFcmTokenAsync(user.Id, updateFcmTokenDto);

        return Ok(result);
    }

}
