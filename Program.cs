using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WaitifyApi.Entities;
using WaitifyApi.Data;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using WaitifyApi.Repositories;
using WaitifyApi.Services;
using Microsoft.OpenApi;
using WaitifyApi.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}).AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Waitify API",
        Description = "API de l'application `waitify.fr`.",
        Contact = new OpenApiContact
        {
            Name = "Développeur (Steven YAMBOS)",
            Url = new Uri("https://www.linkedin.com/in/steven-yambos/")
        },
        Version = "v1"
    });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Entrer un JWT valide",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(document => new() { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
});
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

var jwtSecret = Environment.GetEnvironmentVariable("AppSettingsToken");
var key = Encoding.ASCII.GetBytes(jwtSecret);
var issuer = Environment.GetEnvironmentVariable("AppSettingsIssuer");
var audience = Environment.GetEnvironmentVariable("AppSettingsAudience");
var databaseConfig = Environment.GetEnvironmentVariable("DatabaseConnection");
var azureBlobStorageConnStrg = Environment.GetEnvironmentVariable("AzureBlobStorage");
var auhtenticationGoogleClientId = Environment.GetEnvironmentVariable("AuhtenticationGoogleClientId");
var auhtenticationGoogleSecret = Environment.GetEnvironmentVariable("AuhtenticationGoogleSecret");

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddCookie().AddGoogle(options =>
{
    if (auhtenticationGoogleClientId == null)
    {
        throw new ArgumentNullException(nameof(auhtenticationGoogleClientId));
    }
    if (auhtenticationGoogleSecret == null)
    {
        throw new ArgumentNullException(nameof(auhtenticationGoogleSecret));
    }

    options.ClientId = auhtenticationGoogleClientId;
    options.ClientSecret = auhtenticationGoogleSecret;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SaveTokens = true;
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
    options.ClaimActions.MapJsonKey("urn:google:profile", "link");
    options.ClaimActions.MapJsonKey("image", "picture");
    options.Scope.Add("profile");
    options.Events.OnCreatingTicket = (context) =>
    {
        var picture = context.User.GetProperty("picture").GetString();

        context.Identity.AddClaim(new Claim("picture", picture));

        return Task.CompletedTask;
    };
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = msg =>
        {
            msg.Token = msg.Request.Cookies["waitify_token"];
            var token = msg?.Request.Headers.Authorization.ToString();
            string path = msg?.Request.Path ?? "";
            if (!string.IsNullOrEmpty(msg.Token))

            {
                Console.WriteLine("Access token");
                Console.WriteLine($"URL: {path}");
                Console.WriteLine($"JWT Token: {token}\r\n");
                Console.WriteLine($"Google Token: {msg.Token}\r\n");
            }
            else
            {
                Console.WriteLine("Access token");
                Console.WriteLine("URL: " + path);
                Console.WriteLine("Token: No access token provided\r\n");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            Console.WriteLine();
            Console.WriteLine("Claims from the access token");
            if (ctx?.Principal != null)
            {
                foreach (var claim in ctx.Principal.Claims)
                {
                    Console.WriteLine($"{claim.Type} - {claim.Value}");
                }
            }
            Console.WriteLine();
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine($"❌ Auth failed: {ctx.Exception.GetType().Name} - {ctx.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = ctx =>
        {
            Console.WriteLine($"⚠️ Challenge: Error={ctx.Error}, Description={ctx.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
    options.IncludeErrorDetails = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Injection des dépendances
builder.Services.AddScoped<IAuthRepository, AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserService>();
builder.Services.AddScoped<IBusinessRepository, BusinessService>();
builder.Services.AddScoped<IQueueRepository, QueueService>();
builder.Services.AddScoped<QRCodeGeneratorService>();
builder.Services.AddScoped<IEmailRepository, EmailService>();
builder.Services.AddScoped<IContactRepository, ContactService>();


builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();


builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(databaseConfig));
builder.Services.AddSingleton(x => new BlobServiceClient(azureBlobStorageConnStrg));

/* 
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
{
    context.HttpContext.Response.StatusCode = 429;
    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
    {
        await context.HttpContext.Response.WriteAsync(
            $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s). " +
            $"Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
    }
    else
    {
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later. " +
            "Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
    }
};
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
}); 
*/

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:5500").AllowAnyMethod().AllowAnyHeader()
            .AllowCredentials();
        });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleHelper.EnsureRolesCreated(roleManager);
}

// app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSerilogRequestLogging();

app.Run();
