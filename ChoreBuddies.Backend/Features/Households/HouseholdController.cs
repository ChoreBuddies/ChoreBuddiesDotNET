using AutoMapper;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authentication;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Households;

[ApiController]
[Route("api/v1/household")]
[Authorize]
public class HouseholdController(IHouseholdService service, IAuthService authService, IMapper mapper) : Controller
{
    private readonly IHouseholdService _service = service;
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;
    // Create
    [HttpPost("create")]
    public async Task<IActionResult> CreateHousehold([FromBody] CreateHouseholdDto createHouseholdDto)
    {
        var household = await _service.CreateHouseholdAsync(createHouseholdDto);
        if (household != null)
        {
            return Ok(_mapper.Map<HouseholdDto>(household));
        }
        else
        {
            return BadRequest();
        }
    }
    // Read
    [HttpGet("{householdId}")]
    public async Task<IActionResult> GetUsersHousehold(int householdId)
    {
        var household = await _service.GetUsersHouseholdAsync(householdId);
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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidOperationException("Invalid user identifier.");
        }

        var household = await _service.JoinHouseholdAsync(joinHouseholdDto.InvitationCode, userId);
        if (household == null)
        {
            return BadRequest();
        }

        (string accessToken, string refreshToken) = await _authService.GenerateTokensAsync(userId);

        return Ok(new AuthResponseDto(accessToken, refreshToken));
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
