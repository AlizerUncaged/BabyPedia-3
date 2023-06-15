namespace BabyPedia.Models;

public class Appointment
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string AppointmentDate { get; set; }
    
    public string AppointmentTime { get; set; }

    public string? MeetLink { get; set; }

    public string? Prescription { get; set; }

    public AppointmentPayment Payment { get; set; }
    public AppointmentType AppointmentType { get; set; }

    public DateTime DateTimeCreated { get; set; } = DateTime.Today;

    public Parent Parent { get; set; }

    public Child Child { get; set; }

    public PartneredPedia Pedia { get; set; }
}