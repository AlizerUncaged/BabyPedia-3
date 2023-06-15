using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BabyPedia.Models;

public class Child
{
    public long Id { get; set; }

    public string? Username => $"{FirstName} {LastName}";
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PlaceOfBirth { get; set; }

    [DataType(DataType.PhoneNumber)]
    [DisplayName("Birth Height")]
    public double? BirthHeight { get; set; }

    [DataType(DataType.PhoneNumber)]
    [DisplayName("Birth Weight")]
    public double? BirthWeight { get; set; }

    public string? Sex { get; set; }


    public DateTime? DateTimeCreated { get; set; }

    public List<ImmunizationRecord> ImmunizationRecord { get; set; } =
        new List<ImmunizationRecord>();

    public Parent Parent { get; set; }

    //add pedia ID

    public List<Appointment> Appointments { get; set; } =
        new List<Appointment>();
}