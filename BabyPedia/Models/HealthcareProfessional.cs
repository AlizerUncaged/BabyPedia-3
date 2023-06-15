using Microsoft.AspNetCore.Identity;

namespace BabyPedia.Models;

public class HealthcareProfessional : IdentityUser
{
    public string Sex { get; set; }

    public DateTime BirthDate { get; set; }
    
    public DateTime DateTimeCreated { get; set; } = DateTime.Now;
}