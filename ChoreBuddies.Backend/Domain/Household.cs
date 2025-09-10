using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChoreBuddies.Backend.Domain;

public class Household(int ownerId, string name, string? description)
{
    public int Id { get; set; }
    public int OwnerId { get; set; } = ownerId;

    [MaxLength(50, ErrorMessage = "Household name must be 1-50 characters"), MinLength(1)]
    public string Name { get; set; } = name;

    [MaxLength(250, ErrorMessage = "Household description must be less than 250 characters")]
    public string? Description { get; set; } = description;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties

    [JsonIgnore]
    public virtual ICollection<AppUser> Users { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<Chore> Chores { get; set; } = [];

}

