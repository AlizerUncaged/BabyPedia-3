using Microsoft.AspNetCore.Identity;

namespace BabyPedia.Models;

public class Parent : IdentityUser
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Address { get; set; }

    public DateTime DateTimeCreated { get; set; } = DateTime.Now;

    public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    public List<AppointmentPayment> AppointmentPayments { get; set; } = new List<AppointmentPayment>();
    
    public List<Child> Children { get; set; } = new List<Child>();
}