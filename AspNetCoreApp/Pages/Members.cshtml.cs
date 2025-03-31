using AspNetCoreApp.Data;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Pages;

public class MembersModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public MembersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Member> Members { get; set; } = [];

    public async Task OnGetAsync()
    {
        Members = await _context.Members
            .Include(m => m.User) // ✅ Explicitly include IdentityUser
            .ToListAsync();
    }
}
