using AutoMapper;
using Shared.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Features.Users;

[ApiController]
[Route("/api/v1/users")]
public class AppUserController(IAppUserService userService, IMapper mapper) : ControllerBase
{
    private readonly IAppUserService _userService = userService;
    private readonly IMapper _mapper = mapper;

    //[Authorize]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserAsync(int userId)
    {
        //var emailJWT = User.FindFirst(ClaimTypes.Email)?.Value;

        var user = await _userService.GetUserByIdAsync(userId);
        //if (user == null || string.IsNullOrEmpty(emailJWT) || user.Email != emailJWT)
        //    return BadRequest();

        return Ok(_mapper.Map<AppUserDto>(user));
    }

    //[Authorize]
    //[HttpGet("me")]
    //public async Task<IActionResult> GetMeAsync()
    //{
    //    var emailJWT = User.FindFirst(ClaimTypes.Email)?.Value;
    //    if (emailJWT == null)
    //        return BadRequest();
    //    var user = await _userService.GetUserByEmailAsync(emailJWT);
    //    if (user == null)
    //        return BadRequest();

    //    return Ok(_mapper.Map<AppUserDto>(user));
    //}

    //[Authorize]
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUserAsync(int userId, [FromBody] UpdateAppUserDto user)
    {
        //var emailJWT = User.FindFirst(ClaimTypes.Email)?.Value;

        //var userDb = await _userService.GetUserByIdAsync(userId);
        //if (userDb == null || string.IsNullOrEmpty(emailJWT) || userDb.Email != emailJWT)
        //    return BadRequest();

        var result = await _userService.UpdateUserAsync(userId, user);

        return Ok(result);
    }
}
