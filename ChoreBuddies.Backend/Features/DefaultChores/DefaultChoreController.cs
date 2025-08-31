using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Features.DefaultChores
{
    [ApiController]
    [Route("api/defaultChores")]
    public class DefaultChoreController(IDefaultChoreService service, IMapper mapper) : Controller
    {
        private IDefaultChoreService _service = service;
        private IMapper _mapper = mapper;

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDefaultChores()
        {
            var chores = await _service.GetAllDefaultChoresAsync();
            return Ok(_mapper.Map<List<DefaultChoreDto>>(chores));
        }
    }
}








