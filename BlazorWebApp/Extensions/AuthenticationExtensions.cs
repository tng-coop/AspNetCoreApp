// Auto-generated from Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;

namespace BlazorWebApp.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        // Prepare RSA key for JWT validation
        var privateKeyPem = Encoding.UTF8.GetString(Convert.FromBase64String(
            configuration["JwtSettings:PrivateKeyBase64"]!));
        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        services.AddControllers();
        var rsaKey = new RsaSecurityKey(rsa);
        
        services.AddScoped<JwtTokenService>();

        // Authentication configuration
        services.AddAuthentication()
            .AddGitHub(options =>
            {
                options.ClientId = configuration["Authentication:GitHub:ClientId"]!;
                options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"]!;
                options.Scope.Add("user:email");
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
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
                options.ClientId = configuration["Authentication:LINE:ClientId"]!;
                options.ClientSecret = configuration["Authentication:LINE:ClientSecret"]!;
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
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidateLifetime = true
                };
            });

        // Define the Bearer policy for JWT
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                JwtBearerDefaults.AuthenticationScheme,
                policy => policy
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
            );
        });

        return services;
    }
}
