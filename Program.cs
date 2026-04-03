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
}).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = msg =>
        {
            var token = msg?.Request.Headers.Authorization.ToString();
            string path = msg?.Request.Path ?? "";
            if (!string.IsNullOrEmpty(token))

            {
                Console.WriteLine("Access token");
                Console.WriteLine($"URL: {path}");
                Console.WriteLine($"Token: {token}\r\n");
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
// builder.Services.AddScoped<IContactService, ContactService>();
// builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(databaseConfig));
builder.Services.AddSingleton(x => new BlobServiceClient(azureBlobStorageConnStrg));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
        });
});

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
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSerilogRequestLogging();

app.Run();
