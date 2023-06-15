using Microsoft.AspNetCore.Identity;

namespace BabyPedia.Models;

public class PartneredPedia : IdentityUser
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Address { get; set; }

    public string ContactNumber { get; set; } = string.Empty;

    public string PrcLicenseNumber { get; set; } = string.Empty;

    public DateTime PrcExpiration { get; set; }

    public string SubscriptionStatus { get; set; } = string.Empty;

    public List<Appointment> Appointments { get; set; } = new List<Appointment>();

    public DateTime DateTimeCreated { get; set; } = DateTime.Today;

    public List<PediaPayment> PediaPayments { get; set; } =
        new();

    public string? SignatureLink { get; set; }

    public bool IsVerified { get; set; } = false;
}