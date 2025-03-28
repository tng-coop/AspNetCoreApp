using Microsoft.AspNetCore.Identity;
using AspNetCoreApp.Data;
using AspNetCoreApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AspNetCoreApp.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Explicitly configure SMTP settings from IConfiguration (no env vars)
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

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
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
    options.Scope.Add("user:email");
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
});

// Forwarded Headers Configuration (only production)
if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

var app = builder.Build();

// Use Forwarded Headers only in production (Render)
if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Test"))
{
    app.UseHttpsRedirection();
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

app.MapPost("/api/register", async (
    UserManager<IdentityUser> userManager,
    ApplicationDbContext dbContext,
    IConfiguration config,
    RegistrationRequest registrationRequest) =>
{
    var existingUser = await userManager.FindByEmailAsync(registrationRequest.Email);
    if (existingUser != null)
        return Results.BadRequest(new { Message = "Email already registered." });

    var newUser = new IdentityUser
    {
        UserName = registrationRequest.Email,
        Email = registrationRequest.Email,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(newUser, registrationRequest.Password);
    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    await userManager.AddToRoleAsync(newUser, "Member");

    dbContext.Members.Add(new Member
    {
        FirstName = registrationRequest.FirstName,
        LastName = registrationRequest.LastName,
        Email = registrationRequest.Email,
        UserId = newUser.Id
    });
    await dbContext.SaveChangesAsync();

    return Results.Ok(new { Message = "Registration successful." });
})
.WithName("ApiRegister")
.WithOpenApi()
.AllowAnonymous();
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
record RegistrationRequest(string FirstName, string LastName, string Email, string Password);

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

public partial class Program { }
