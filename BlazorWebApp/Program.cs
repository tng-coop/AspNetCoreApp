using System;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Components;
using BlazorWebApp.Components.Account;
using BlazorWebApp.Data;
using BlazorWebApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using BlazorWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Prepare RSA key for JWT validation
var privateKeyPem = Encoding.UTF8.GetString(Convert.FromBase64String(
    builder.Configuration["JwtSettings:PrivateKeyBase64"]!));
var rsa = RSA.Create();
rsa.ImportFromPem(privateKeyPem);
var rsaKey = new RsaSecurityKey(rsa);

builder.Services.AddScoped<JwtTokenService>();

// Add HTTP and application services
builder.Services.AddHttpClient();
builder.Services.AddScoped<INameService, NameService>();
builder.Services.AddScoped<INoteService, NoteService>();

// Add Razor Components and Authentication State
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Configure PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity setup with Default UI and Token Providers
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Authentication configuration
builder.Services.AddAuthentication()
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
        options.Scope.Add("user:email");
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture");
        options.SaveTokens = true;
    })
    .AddOAuth("LINE", options =>
    {
        options.ClientId = builder.Configuration["Authentication:LINE:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:LINE:ClientSecret"]!;
        options.CallbackPath = new PathString("/signin-line");

        options.AuthorizationEndpoint = "https://access.line.me/oauth2/v2.1/authorize";
        options.TokenEndpoint = "https://api.line.me/oauth2/v2.1/token";
        options.UserInformationEndpoint = "https://api.line.me/v2/profile";

        options.Scope.Add("profile");
        options.Scope.Add("openid");
        options.SaveTokens = true;

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "userId");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "displayName");
        options.ClaimActions.MapJsonKey("urn:line:picture", "pictureUrl");

        options.Events.OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            context.RunClaimActions(user.RootElement);
        };
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaKey,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true
        };
    });

// Define the Bearer policy for JWT
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        JwtBearerDefaults.AuthenticationScheme,
        policy => policy
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
    );
});

// Forwarded Headers in production
if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Localization and HttpContextAccessor
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddHttpContextAccessor();

// Configure custom AuthenticationOptions display names
builder.Services.Configure<AuthenticationOptions>(opts =>
{
    opts.Schemes.First(s => s.Name == "LINE").DisplayName = "LINE";
});

var app = builder.Build();

// Use Forwarded Headers and HSTS in production
if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseHsts();
}

app.UseHttpsRedirection();

// Migrations and error handling
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// API endpoints
app.MapGet("/api/name/{key}", async (
        string key,
        ClaimsPrincipal user,
        INameService svc) =>
{
    var value = await svc.GetLatestForNameAsync(key);
    return value is not null
        ? Results.Ok(value)
        : Results.NotFound();
})
.RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

app.MapPut("/api/name/{key}", async (
        string key,
        NameWriteDto dto,
        ClaimsPrincipal user,
        INameService svc) =>
{
    await svc.SetNameAsync(key, dto.Value, null);
    return Results.NoContent();
})
.RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

app.MapGet("/api/hello", () => Results.Ok("Hello from API!"))
.RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

// Blazor render
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Data seeding
using (var scope = app.Services.CreateScope())
{
    var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var nameSvc     = scope.ServiceProvider.GetRequiredService<INameService>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Apply migrations
    db.Database.Migrate();

    // Seed roles
    foreach (var role in new[] { "Member", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Pull default password from env-var
    var defaultPassword = builder.Configuration["DefaultUser:Password"]
                          ?? throw new InvalidOperationException(
                                "DefaultUser__Password environment variable is not set");

    // Seed default Admin user
    const string adminEmail = "admin@yourdomain.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            EmailConfirmed = true
        };
        if ((await userManager.CreateAsync(adminUser, defaultPassword)).Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Seed default Member user
    const string memberEmail = "member@yourdomain.com";
    var memberUser = await userManager.FindByEmailAsync(memberEmail);
    if (memberUser == null)
    {
        memberUser = new ApplicationUser
        {
            UserName       = memberEmail,
            Email          = memberEmail,
            EmailConfirmed = true
        };
        if ((await userManager.CreateAsync(memberUser, defaultPassword)).Succeeded)
            await userManager.AddToRoleAsync(memberUser, "Member");
    }

    // Seed video links
    var videoSeeds = new Dictionary<string, string>
    {
        ["video1"] = "https://vimeo.com/1078878884",
        ["video2"] = "https://vimeo.com/1078878886",
        ["video3"] = "https://vimeo.com/1078878889",
        ["video4"] = "https://vimeo.com/1078878893",
        ["video5"] = "https://vimeo.com/1078878899",
        ["video6"] = "https://vimeo.com/1078878852",
        ["video7"] = "https://vimeo.com/1078878865",
        ["video8"] = "https://vimeo.com/1078878869",
        ["video9"] = "https://vimeo.com/1078878875",
        ["video10"] = "https://vimeo.com/1078878880",
    };  

    foreach (var (key, url) in videoSeeds)
    {
        if (await nameSvc.GetLatestForNameAsync(key) is null)
            await nameSvc.SetNameAsync(key, url, ownerId: null);
    }
}

app.Run();