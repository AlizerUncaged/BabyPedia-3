using Microsoft.AspNetCore.Identity;

namespace BabyPedia.Models;

public class User : IdentityUser
{
    public string? Address { get; set; }

    public DateTime LastLogin { get; set; } = DateTime.Now;
    
    public DateTime DateTimeJoined { get; set; } = DateTime.Now;

    public Parent? Parent { get; set; }
}