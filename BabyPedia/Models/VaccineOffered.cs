namespace BabyPedia.Models;

public class VaccineOffered
{
    public long Id { get; set; }
    public bool Availability { get; set; }
    public double Price { get; set; }
    public PartneredPedia PediaId { get; set; }
    public Vaccine Vaccine { get; set; }
    public DateTime DateTime { get; set; }
}