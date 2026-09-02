using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Enums;
using WaitifyApi.Models;
using WaitifyApi.Repositories;
using WaitifyApi.Services;
using Google.Apis.Auth;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WaitifyApi.Controllers
{
    [Route("api/auth")]
    [EnableRateLimiting("fixed")]
    [ApiController]
    public class AuthController(
        IAuthRepository authService,
        IAccountService accountService,
        IApplicationUserRepository userService,
        TokenService tokenService,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthController> logger
    ) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            if (request.ProfilePicture?.Length > 1 * 1024 * 1024)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "La taille du fichier ne doit pas excéder 1MB.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, token, errors) = await authService.RegisterAsync(request);

            if (!success)
            {
                logger.LogWarning("Échec de l'inscription pour {Email}", request.Email);
                return BadRequest(new { errors });
            }

            return CreatedAtAction(nameof(Register), new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, tokens, error) = await authService.LoginAsync(request);

            if (!success)
            {
                logger.LogWarning("Échec de connexion pour {Email}", request.Email);
                return Unauthorized(new { error });
            }

            logger.LogInformation("Token : {@0}", tokens?.AccessToken);
            return Ok(tokens);
        }

        [HttpGet("login/google")]
        public IActionResult GoogleLogin([FromQuery] string returnUrl, LinkGenerator linkGenerator)
        {
            var callbackUrl = linkGenerator.GetPathByName(HttpContext, "GoogleLoginCallback")
                + $"?returnUrl={Uri.EscapeDataString(returnUrl)}";

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                "Google", callbackUrl);

            logger.LogInformation("CALLBACK URL : {@0}", callbackUrl);

            return Challenge(properties, ["Google"]);
        }

        [HttpGet("google/callback")]
        [EndpointName("GoogleLoginCallback")]
        public async Task<IActionResult> GoogleLoginCallback([FromQuery] string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return Unauthorized();

            var jwtToken = await authService.LoginWithGoogleAsync(result.Principal);

            string authUrl = returnUrl + "?token=" + Uri.EscapeDataString(jwtToken);

            logger.LogInformation("RETURN URL : {@0}", returnUrl);

            return Redirect(authUrl);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, tokens, error) = await authService.RefreshTokensAsync(request);

            if (!success)
                return Unauthorized(new { error });

            return Ok(tokens);
        }

        /*         [HttpGet]
                [Authorize(AuthenticationSchemes = "Bearer")]
                public IActionResult AuthenticatedOnly()
                {
                    var username = User.Identity?.Name;
                    logger.LogInformation("✅ Endpoint authentifié accessible par {User}", username);
                    return Ok(new { message = "Vous êtes authentifié.", user = username });
                }

                [HttpGet("admin")]
                [Authorize(AuthenticationSchemes = "Bearer", Roles = nameof(Role.Admin))]
                public IActionResult AdminOnly()
                {
                    var username = User.Identity?.Name;
                    logger.LogInformation("✅ Endpoint admin accessible par {User}", username);
                    return Ok(new { message = "Vous êtes Admin.", user = username });
                } */
    }
}
