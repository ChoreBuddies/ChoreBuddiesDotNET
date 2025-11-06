using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Households;

[ApiController]
[Route("api/v1/household")]
public class HouseholdController(IHouseholdService service, IMapper mapper) : Controller
{
    private readonly IHouseholdService _service = service;
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
        if (household != null)
        {
            return Ok(_mapper.Map<HouseholdDto>(household));
        }
        else
        {
            return BadRequest();
        }
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
