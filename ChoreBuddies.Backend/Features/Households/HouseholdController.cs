using AutoMapper;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authentication;
using System.Security.Claims;

namespace Shared.Households;

[ApiController]
[Route("api/v1/household")]
[Authorize]
public class HouseholdController(IHouseholdService service, IMapper mapper, IAuthService authService, ITokenService tokenService) : Controller
{
    private readonly IHouseholdService _service = service;
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    // Create
    [HttpPost("add")]
    public async Task<IActionResult> CreateHousehold([FromBody] CreateHouseholdDto createHouseholdDto)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var household = await _service.CreateHouseholdAsync(createHouseholdDto, userId);
        if (household == null)
        {
            return BadRequest();
        }

        string accessToken = await _authService.GenerateAccessTokenAsync(userId);

        return Ok(new AuthResponseDto(accessToken, ""));
    }
    // Read
    [HttpGet]
    public async Task<IActionResult> GetUsersHousehold([FromQuery] int? householdId)
    {
        if (householdId is null)
        {
            householdId = _tokenService.GetHouseholdIdFromToken(User);
        }
        var household = await _service.GetUsersHouseholdAsync((int)householdId);
        if (household != null)
        {
            return Ok(_mapper.Map<HouseholdDto>(household));
        }
        else
        {
            return BadRequest();
        }
    }
    // Update
    [HttpPut("update/{householdId}")]
    public async Task<IActionResult> UpdateHousehold(int householdId, [FromBody] CreateHouseholdDto createHouseholdDto)
    {
        var household = await _service.UpdateHouseholdAsync(householdId, createHouseholdDto);
        if (household != null)
        {
            return Ok(_mapper.Map<HouseholdDto>(household));
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpPut("join")]
    public async Task<IActionResult> JoinHousehold([FromBody] JoinHouseholdDto joinHouseholdDto)
    {
        var userId = _tokenService.GetUserIdFromToken(User);

        var household = await _service.JoinHouseholdAsync(joinHouseholdDto.InvitationCode, userId);
        if (household == null)
        {
            return BadRequest();
        }

        string accessToken = await _authService.GenerateAccessTokenAsync(userId);

        return Ok(new AuthResponseDto(accessToken, ""));
    }
    // Delete
    [HttpDelete("{householdId}")]
    public async Task<IActionResult> DeleteHousehold(int householdId)
    {
        var household = await _service.DeleteHouseholdAsync(householdId);
        if (household != null)
        {
            return Ok(_mapper.Map<HouseholdDto>(household));
        }
        else
        {
            return BadRequest();
        }
    }
}
