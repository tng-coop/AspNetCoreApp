namespace AspNetCoreApp.Models;

public class Member
{
    public int Id { get; set; }            // Primary key
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
}
