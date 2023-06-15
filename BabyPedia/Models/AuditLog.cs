using System.ComponentModel.DataAnnotations;

namespace BabyPedia.Models;

public class AuditLog
{
    [Key]
    public string Id { get; set; }

    public string? Table { get; set; }
    
    public string? TableColumn { get; set; }
    
    
    public string? RowId { get; set; }
    
    public string? OldValue { get; set; }
    
    public string? NewValue { get; set; }
    
    public string? UserId { get; set; }
    
    public DateTime UpdateTime { get; set; } = DateTime.Now;
}