using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend
{
    [ApiController]
    [Route("api/{controller}")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Weather()
        {
            return Ok(new HomeObject("Welcome!"));
        }
        protected class HomeObject
        {
            public string? message { get; set; }
            public HomeObject(string? Message)
            {
                message = Message;
            }
        }
    }
}