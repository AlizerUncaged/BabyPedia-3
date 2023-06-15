namespace BabyPedia.Models;

public class AppointmentType
{
    public long Id { get; set; }

    public string Name { get; set; }
    
    public DateTime DateTimeCreated { get; set; } = DateTime.Now;
}