namespace BabyPedia.Models;

public class AppointmentPayment
{
    public long Id { get; set; }

    public double Amount { get; set; }

    public string? Status { get; set; }

    public Parent? Parent { get; set; }

    public DateTime DateTimeCreate { get; set; } = DateTime.Today;
}