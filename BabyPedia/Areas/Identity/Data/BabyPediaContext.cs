using System.Data.Entity.Infrastructure;
using BabyPedia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BabyPedia.Data;

public class BabyPediaContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Appointment> Appointments { get; set; }

    public DbSet<AppointmentPayment> AppointmentPayments { get; set; }
    public DbSet<PediaPayment> PediaPayments { get; set; }
    public DbSet<Chat> Chats { get; set; }

    public DbSet<AppointmentType> AppointmentTypes { get; set; }

    public DbSet<Child> Children { get; set; }

    public DbSet<HealthcareProfessional> HealthcareProfessionals { get; set; }

    public DbSet<ImmunizationRecord> ImmunizationRecords { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Parent> Parents { get; set; }

    public DbSet<PartneredPedia> PartneredPedias { get; set; }

    public DbSet<User> SiteUsers { get; set; }

    public DbSet<Vaccine> Vaccines { get; set; }

    public DbSet<VaccineOffered> VaccinesOffered { get; set; }

    public BabyPediaContext(DbContextOptions<BabyPediaContext> options)
        : base(options)
    {
    }

    public static string? CurrentUserId  = null;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Parent>()
            .HasMany<Appointment>();

        builder.Entity<AuditLog>()
            .Property(l => l.Id)
            .ValueGeneratedOnAdd();

        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog)
                continue;

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary || property.Metadata.IsPrimaryKey())
                    continue;

                AuditLog log = new()
                {
                    Table = entry.Metadata.GetTableName(),
                    RowId = entry.Metadata.GetKeys().First().ToString(),
                    UserId = CurrentUserId,
                    Id = Guid.NewGuid().ToString()
                };


                log.TableColumn = property.Metadata.Name;
                log.OldValue = property.OriginalValue?.ToString();
                log.NewValue = property.CurrentValue?.ToString();
                log.UserId = CurrentUserId;

                await AuditLogs.AddAsync(log, cancellationToken);
            }
        }
        var saved = await base.SaveChangesAsync(cancellationToken);

        return saved;
    }
}