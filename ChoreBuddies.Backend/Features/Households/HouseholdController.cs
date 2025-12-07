using AutoMapper;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Features.Households;

[ApiController]
[Route("api/v1/household")]
[Authorize]
public class HouseholdController(IHouseholdService service, IMapper mapper, ITokenService tokenService) : Controller
{
    private readonly IHouseholdService _service = service;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    // Create
    [HttpPost("create")]
    public async Task<IActionResult> CreateHousehold([FromBody] CreateHouseholdDto createHouseholdDto)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var household = await _service.CreateHouseholdAsync(createHouseholdDto, userId);
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
        var userId = tokenService.GetUserIdFromToken(User);

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
