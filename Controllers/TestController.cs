using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using WaitifyApi.Services;

namespace WaitifyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(ILogger<TestController> logger) : ControllerBase
    {

        [HttpGet]
        [EnableRateLimiting("fixed")]
        public async Task<string> WelcomeMessage()
        {
            var message = "Hello World !";
            Console.WriteLine(message);
            logger.LogInformation("Token de connexion : {0}", JsonConvert.SerializeObject(message, Formatting.Indented));

            return message;
        }
    }
}