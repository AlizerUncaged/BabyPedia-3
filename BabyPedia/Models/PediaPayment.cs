namespace BabyPedia.Models;

public class PediaPayment
{
    public long Id { get; set; }

    public double Payment { get; set; }

    public PartneredPedia? PartneredPedia { get; set; }

    public DateTime DatePaid { get; set; } = DateTime.Now;
}