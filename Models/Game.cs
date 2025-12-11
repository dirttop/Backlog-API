using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BacklogAPI.Models;

public class Game
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Required]
    public int SteamAppId { get; set; }
    public string Title { get; set; } = string.Empty; 
    public string? Genre { get; set; }
    public string? Developer { get; set; }
    public int? ReleaseYear { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedOn { get; set; }
    public bool Dropped { get; set; }
    public float? PlaytimeHours { get; set; }
    public float? Rating { get; set; }
    public string? Review { get; set; }
    public DateTime? ValidatedOn { get; set;}
}

