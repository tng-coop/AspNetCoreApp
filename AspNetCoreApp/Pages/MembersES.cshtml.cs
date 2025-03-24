using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCoreApp.Pages;

[Authorize]
public class MembersESModel : PageModel
{
    public void OnGet()
    {
        // No server-side logic needed since data is fetched via AJAX
    }
}
