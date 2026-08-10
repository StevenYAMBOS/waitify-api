using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using WaitifyApi.Helpers;
using WaitifyApi.Services;

namespace WaitifyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(ILogger<TestController> logger) : ControllerBase
    {

        [HttpGet]
        // [Authorize(AuthenticationSchemes = "Bearer")]
        [EnableRateLimiting("fixed")]
        public async Task<string> WelcomeMessage()
        {
            var message = "Hello World !";
            Console.WriteLine(message);
            logger.LogInformation("Message : {@0}", JsonResponseHelper.JsonConversion(message));

            return message;
        }
    }
}