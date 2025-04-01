using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace AspNetCoreApp.Pages
{
    public class UploadModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public IFormFile? uploadFile { get; set; }

        public string Message { get; set; } = "";

        public UploadModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnPostAsync()
        {
            if (uploadFile == null || uploadFile.Length == 0)
            {
                Message = "Please select a file to upload.";
                return;
            }

            var client = _httpClientFactory.CreateClient();

            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(uploadFile.OpenReadStream());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(uploadFile.ContentType);
            content.Add(fileContent, "file", uploadFile.FileName);

            var response = await client.PostAsync("https://your-php-server.com/a.php", content);

            if (response.IsSuccessStatusCode)
            {
                Message = await response.Content.ReadAsStringAsync();
            }
            else
            {
                Message = $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            }
        }
    }
}
