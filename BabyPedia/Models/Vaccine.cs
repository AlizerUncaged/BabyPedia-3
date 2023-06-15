namespace BabyPedia.Models;

public class Vaccine
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string LotNumber { get; set; }

    public string Manufacturer { get; set; }

    public DateTime ExpirationDate { get; set; }

    public string Disease { get; set; }

    public string DiseaseSymptoms { get; set; }

    public string DiseaseComplications { get; set; }

    public double RecommendedAge { get; set; }

    public DateTime DateTimeCreated { get; set; }
}