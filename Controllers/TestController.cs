using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WaitifyApi.Services;

namespace WaitifyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController() : ControllerBase
    {

        [HttpGet]
        [EnableRateLimiting("fixed")]
        public async Task<string> WelcomeMessage()
        {
            var message = "Hello World !";
            Console.WriteLine(message);
            // await blobService.ListBlobsFlatListing();

            return message;
        }
    }
}