using Microsoft.AspNetCore.Identity;
using AspNetCoreApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AspNetCoreApp.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load smtp.env if it exists
if (File.Exists("smtp.env"))
{
    Env.Load("smtp.env");
}

builder.Services.Configure<SmtpSettings>(options =>
{
    options.Server = Environment.GetEnvironmentVariable("SMTP_SERVER")
        ?? builder.Configuration["SmtpSettings:Server"]
        ?? "localhost";

    options.Port = int.TryParse(
        Environment.GetEnvironmentVariable("SMTP_PORT")
        ?? builder.Configuration["SmtpSettings:Port"],
        out var port) ? port : 1025;

    options.User = Environment.GetEnvironmentVariable("SMTP_USER")
        ?? builder.Configuration["SmtpSettings:User"]
        ?? string.Empty;

    options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
        ?? builder.Configuration["SmtpSettings:Password"]
        ?? string.Empty;

    options.FromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL")
        ?? builder.Configuration["SmtpSettings:FromEmail"]
        ?? "no-reply@example.com";

    options.UseStartTls = (Environment.GetEnvironmentVariable("USE_STARTTLS")
        ?? builder.Configuration["SmtpSettings:UseStartTls"]
        ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);

        
});

// Register services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
});

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/members", async (ApplicationDbContext dbContext) =>
{
    var members = await dbContext.Members
        .Select(m => new { m.FirstName, m.LastName, m.Email })
        .ToListAsync();

    return Results.Ok(members);
})
.RequireAuthorization("ApiPolicy")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.WithName("GetMembersApi")
.WithOpenApi();

app.MapGet("/weatherforecast", () =>
{
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild",
                            "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

    return Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// JWT login API endpoint
app.MapPost("/api/login", async (
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    IConfiguration config,
    LoginRequest loginRequest) =>
{
    var user = await userManager.FindByEmailAsync(loginRequest.Email);
    if (user is null)
        return Results.Unauthorized();

    var result = await signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true);
    if (!result.Succeeded)
        return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Email, user.Email!)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        Issuer = config["JwtSettings:Issuer"],
        Audience = config["JwtSettings:Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]!)), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);

    return Results.Ok(new { Message = "Login successful", Token = jwt });
})
.WithName("ApiLogin")
.WithOpenApi();

// Initialize Database with migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record LoginRequest(string Email, string Password);

// SmtpSettings class
public class SmtpSettings
{
    public string Server { get; set; } = "";
    public int Port { get; set; }
    public string FromEmail { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public bool UseStartTls { get; set; }
}