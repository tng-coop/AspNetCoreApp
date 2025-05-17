using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorWebApp.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
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
            });

        return services;
    }
}
