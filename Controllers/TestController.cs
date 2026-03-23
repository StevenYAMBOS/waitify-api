using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Portfolio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController() : ControllerBase
    {

        [HttpGet]
        [EnableRateLimiting("fixed")]
        public string WelcomeMessage()
        {
            var message = "Page test";
            Console.WriteLine(message);
            return message;
        }
    }
}