using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Features.Rewards;
[Route("api/v1/rewards")]
[ApiController]
[Authorize]
public class RewardsController : ControllerBase
{
}
