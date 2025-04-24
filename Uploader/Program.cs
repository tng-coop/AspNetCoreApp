using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorWebApp.Services;    // ← JwtTokenService

namespace Uploader
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: dotnet Uploader.dll <path-to-image.png>");
                return;
            }

            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"ERROR: {filePath} not found.");
                return;
            }

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                       .AddEnvironmentVariables();
                })
                .ConfigureServices((ctx, svc) =>
                {
                    svc.AddSingleton<JwtTokenService>();
                    svc.AddHttpClient();
                })
                .Build();

            var config   = host.Services.GetRequiredService<IConfiguration>();
            var jwtSvc   = host.Services.GetRequiredService<JwtTokenService>();
            var factory  = host.Services.GetRequiredService<IHttpClientFactory>();
            var uploadUrl = config["UploadSettings:Endpoint"] 
                            ?? throw new InvalidOperationException("UploadSettings:Endpoint is not configured.");

            // user settings
            var userId = config["UserSettings:UserId"]!;
            var email  = config["UserSettings:Email"]!;

            var token   = jwtSvc.GenerateToken(userId, email);

            var client  = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var fs      = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fs), "file", Path.GetFileName(filePath));

            var resp = await client.PostAsync(uploadUrl, content);
            var body = await resp.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)resp.StatusCode} {resp.ReasonPhrase}");
            Console.WriteLine(body);
        }
    }
}
