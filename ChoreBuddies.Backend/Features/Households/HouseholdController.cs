using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Features.Households
{
    [ApiController]
    [Route("api/household")]
    public class HouseholdController(IHouseholdService service, IMapper mapper) : Controller
    {
        private readonly IHouseholdService _service = service;
        private readonly IMapper _mapper = mapper;

        [HttpGet("{householdId}")]
        public async Task<IActionResult> GetUsersHousehold(int householdId)
        {
            var household = await _service.GetUsersHouseholdAsync(householdId);
            return Ok(_mapper.Map<HouseholdDto>(household));
        }
    }
}