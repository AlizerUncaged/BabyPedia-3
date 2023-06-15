namespace BabyPedia.Models;

public class ImmunizationRecord
{
    public long Id { get; set; }
    
    public string Vaccine { get; set; }
    
    public double DoseNumber { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Today;
    
    public string AdministeredBy { get; set; }
    
    public string Remarks { get; set; }
    
    public DateTime DateTimeCreated { get; set; } = DateTime.Today;
}