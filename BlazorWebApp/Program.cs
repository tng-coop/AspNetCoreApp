// __SPLT__USINGS_START__
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
using Microsoft.Extensions.FileProviders;
// __SPLT__USINGS_END__

// __SPLT__BUILDER_START__
var builder = WebApplication.CreateBuilder(args);
// __SPLT__BUILDER_END__

// __SPLT__JWT_SETUP_START__
// Prepare RSA key for JWT validation
var privateKeyPem = Encoding.UTF8.GetString(Convert.FromBase64String(
    builder.Configuration["JwtSettings:PrivateKeyBase64"]!));
var rsa = RSA.Create();
rsa.ImportFromPem(privateKeyPem);
builder.Services.AddControllers();
var rsaKey = new RsaSecurityKey(rsa);

builder.Services.AddScoped<JwtTokenService>();
// __SPLT__JWT_SETUP_END__

// __SPLT__HTTP_SERVICES_START__
// Add HTTP and application services
builder.Services.AddHttpClient();
builder.Services.AddScoped<INameService, NameService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IPublicationService, PublicationService>();
// register category service
builder.Services.AddScoped<ICategoryService, CategoryService>();
// __SPLT__HTTP_SERVICES_END__

// __SPLT__BLAZOR_AUTH_START__
// Add Razor Components and Authentication State
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
// __SPLT__BLAZOR_AUTH_END__

// __SPLT__DBCONFIG_START__
// Configure PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// __SPLT__DBCONFIG_END__

// __SPLT__IDENTITY_SETUP_START__
// Identity setup with Default UI and Token Providers
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();
builder.Services.AddTransient<IEmailSender, EmailSender>();
// __SPLT__IDENTITY_SETUP_END__

// __SPLT__AUTH_CONFIG_START__
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
// __SPLT__AUTH_CONFIG_END__

// __SPLT__AUTHORIZATION_POLICY_START__
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
// __SPLT__AUTHORIZATION_POLICY_END__

// __SPLT__FORWARDED_HEADERS_CONFIG_START__
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
// __SPLT__FORWARDED_HEADERS_CONFIG_END__

// __SPLT__LOCALIZATION_START__
// Localization and HttpContextAccessor
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddHttpContextAccessor();

// Configure custom AuthenticationOptions display names
builder.Services.Configure<AuthenticationOptions>(opts =>
{
    opts.Schemes.First(s => s.Name == "LINE").DisplayName = "LINE";
});
// __SPLT__LOCALIZATION_END__

// __SPLT__APP_BUILD_START__
var app = builder.Build();
// __SPLT__APP_BUILD_END__

// __SPLT__PROD_PIPELINE_START__
// Use Forwarded Headers and HSTS in production
if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    var certPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "cert"));
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(certPath),
        RequestPath = "/cert"
    });    
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = new PhysicalFileProvider(certPath),
        RequestPath = "/cert"
    });
}
// __SPLT__PROD_PIPELINE_END__

// __SPLT__MIGRATIONS_ERRORHANDLING_START__
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
// __SPLT__MIGRATIONS_ERRORHANDLING_END__

// __SPLT__STATIC_CACHE_START__
app.UseStaticFiles();
var cachePath = Path.Combine(app.Environment.WebRootPath, "imgcache");
Directory.CreateDirectory(cachePath);
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(cachePath),
    RequestPath = "/imgcache",
    OnPrepareResponse = ctx => ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=604800"
});
// __SPLT__STATIC_CACHE_END__

// __SPLT__ROUTING_AUTH_START__
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseAntiforgery();
// __SPLT__ROUTING_AUTH_END__

// __SPLT__NAME_HELLO_ENDPOINTS_START__
// Name and Hello endpoints
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
// __SPLT__NAME_HELLO_ENDPOINTS_END__

// __SPLT__PUBLICATION_ENDPOINTS_START__
// Publication endpoints
app.MapGet("/api/publications", async (IPublicationService svc) =>
    Results.Ok(await svc.ListAsync()))
    .RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

app.MapGet("/api/publications/{id:guid}", async (Guid id, IPublicationService svc) =>
{
    var pub = await svc.GetAsync(id);
    return pub is not null ? Results.Ok(pub) : Results.NotFound();
}).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

app.MapPost("/api/publications", async (PublicationWriteDto dto, IPublicationService svc) =>
{
    var created = await svc.CreateAsync(dto);
    return Results.Created($"/api/publications/{created.Id}", created);
}).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

app.MapPut("/api/publications/{id:guid}", async (Guid id, PublicationWriteDto dto, IPublicationService svc) =>
{
    await svc.UpdateAsync(id, dto);
    return Results.NoContent();
}).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

app.MapPost("/api/publications/{id:guid}/publish", async (Guid id, IPublicationService svc) =>
{
    await svc.PublishAsync(id);
    return Results.NoContent();
}).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);
// __SPLT__PUBLICATION_ENDPOINTS_END__

// __SPLT__BLAZOR_RENDER_START__
// Blazor render
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();
// __SPLT__BLAZOR_RENDER_END__

// __SPLT__DATA_SEEDING_START__
// Data seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var nameSvc = scope.ServiceProvider.GetRequiredService<INameService>();
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
            UserName = adminEmail,
            Email = adminEmail,
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
            UserName = memberEmail,
            Email = memberEmail,
            EmailConfirmed = true
        };
        if ((await userManager.CreateAsync(memberUser, defaultPassword)).Succeeded)
            await userManager.AddToRoleAsync(memberUser, "Member");
    }

    // Seed second default Member user
    const string secondMemberEmail = "member2@yourdomain.com";
    var secondMemberUser = await userManager.FindByEmailAsync(secondMemberEmail);
    if (secondMemberUser == null)
    {
        secondMemberUser = new ApplicationUser
        {
            UserName = secondMemberEmail,
            Email = secondMemberEmail,
            EmailConfirmed = true
        };
        if ((await userManager.CreateAsync(secondMemberUser, defaultPassword)).Succeeded)
            await userManager.AddToRoleAsync(secondMemberUser, "Member");
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
// __SPLT__DATA_SEEDING_END__

// __SPLT__RUN_START__
app.Run();
// __SPLT__RUN_END__
