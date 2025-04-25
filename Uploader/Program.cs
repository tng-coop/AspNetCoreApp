using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorWebApp.Services;
using System.IdentityModel.Tokens.Jwt;    // for JwtTokenService

namespace Uploader
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            void ShowUsage()
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet Uploader.dll upload <file-path>");
                Console.WriteLine("  dotnet Uploader.dll register <key> <value>");
                Console.WriteLine("  dotnet Uploader.dll token");
                Console.WriteLine("  dotnet Uploader.dll get <key>");
            }

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var mode = args[0].ToLowerInvariant();
            if (mode != "upload" && mode != "register" && mode != "token" && mode != "get")
            {
                Console.Error.WriteLine($"Unknown command: {args[0]}");
                ShowUsage();
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

            var config = host.Services.GetRequiredService<IConfiguration>();
            var jwtSvc = host.Services.GetRequiredService<JwtTokenService>();
            var factory = host.Services.GetRequiredService<IHttpClientFactory>();

            var uploadUrl = config["UploadSettings:Endpoint"]
                             ?? throw new InvalidOperationException("UploadSettings:Endpoint not configured.");
            var nameSvcUrl = config["NameSettings:Endpoint"]
                             ?? throw new InvalidOperationException("NameSettings:Endpoint not configured.");

            // Generate JWT for all operations
            var userId = config["UserSettings:UserId"]!;
            var email = config["UserSettings:Email"]!;
            var token = jwtSvc.GenerateToken(userId, email);

            // … after var token = jwtSvc.GenerateToken(userId, email);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // // Dump the raw token and its claims so you can see exactly what’s in it:
            // Console.WriteLine("=== JWT DUMP ===");
            // Console.WriteLine(token);
            // Console.WriteLine("=== CLAIMS ===");
            // foreach (var c in jwtToken.Claims)
            // {
            //     Console.WriteLine($"  {c.Type} = {c.Value}");
            // }
            // Console.WriteLine("===============");

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (mode == "token")
            {
                Console.WriteLine(token);
                return;
            }
            else if (mode == "get")
            {
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Usage: dotnet Uploader.dll get <key>");
                    return;
                }
                var key   = args[1];
                var resp  = await client.GetAsync($"{nameSvcUrl}/{Uri.EscapeDataString(key)}");
                var body  = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"Get      → {(int)resp.StatusCode} {resp.ReasonPhrase}");
                Console.WriteLine(body);
            }
            else if (mode == "upload")
            {
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Usage: dotnet Uploader.dll upload <file-path>");
                    return;
                }
                var filePath = args[1];
                if (!File.Exists(filePath))
                {
                    Console.Error.WriteLine($"ERROR: {filePath} not found.");
                    return;
                }

                using var fs = File.OpenRead(filePath);
                using var content = new MultipartFormDataContent();
                content.Add(new StreamContent(fs), "file", Path.GetFileName(filePath));

                var resp = await client.PostAsync(uploadUrl, content);
                var body = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"Upload → {(int)resp.StatusCode} {resp.ReasonPhrase}");
                Console.WriteLine(body);
            }
            else // register
            {
                if (args.Length != 3)
                {
                    Console.Error.WriteLine("Usage: dotnet Uploader.dll register <key> <value>");
                    return;
                }
                var key = args[1];
                var value = args[2];

                var dtoJson = JsonSerializer.Serialize(new { value });
                using var nameContent = new StringContent(dtoJson, Encoding.UTF8, "application/json");

                var nameResp = await client.PutAsync(
                    $"{nameSvcUrl}/{Uri.EscapeDataString(key)}",
                    nameContent);
                var nameBody = await nameResp.Content.ReadAsStringAsync();
                Console.WriteLine($"Register → {(int)nameResp.StatusCode} {nameResp.ReasonPhrase}");
                Console.WriteLine(nameBody);
            }
        }
    }
}