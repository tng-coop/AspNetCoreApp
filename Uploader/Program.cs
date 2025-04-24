using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorWebApp.Services;    // ← your JwtTokenService namespace

namespace Uploader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <file1.png> [<file2.jpg> ...]");
                return;
            }

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    // read appsettings.json + environment variables
                    cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                })
                .ConfigureServices((ctx, svc) =>
                {
                    // register your RSA‐based JWT service and HttpClientFactory
                    svc.AddSingleton<JwtTokenService>();
                    svc.AddHttpClient();
                })
                .Build();

            var config  = host.Services.GetRequiredService<IConfiguration>();
            var jwtSvc  = host.Services.GetRequiredService<JwtTokenService>();
            var factory = host.Services.GetRequiredService<IHttpClientFactory>();

            // pull in your hard‐coded user settings
            var userId = config["UserSettings:UserId"] 
                         ?? throw new InvalidOperationException("UserSettings:UserId missing");
            var email  = config["UserSettings:Email"] 
                         ?? throw new InvalidOperationException("UserSettings:Email missing");

            // generate an RSA‐signed JWT
            var token = jwtSvc.GenerateToken(userId, email);

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            const string uploadUrl = "https://tng.coop/photos/a.php";

            foreach (var filePath in args)
            {
                if (!File.Exists(filePath))
                {
                    Console.Error.WriteLine($"[SKIP] '{filePath}' not found.");
                    continue;
                }

                using var fs      = File.OpenRead(filePath);
                using var content = new MultipartFormDataContent();
                content.Add(new StreamContent(fs), "file", Path.GetFileName(filePath));

                Console.Write($"Uploading '{filePath}'… ");
                var resp = await client.PostAsync(uploadUrl, content);
                var body = await resp.Content.ReadAsStringAsync();

                Console.WriteLine($"Status {(int)resp.StatusCode} {resp.ReasonPhrase}");
                Console.WriteLine(body);
                Console.WriteLine(new string('-', 40));
            }
        }
    }
}
