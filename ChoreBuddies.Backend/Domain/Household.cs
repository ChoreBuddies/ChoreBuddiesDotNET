using ChoreBuddies.Backend.Features.Chores;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChoreBuddies.Backend.Domain
{
    public class Household
    {
        public int Id { get; set; }

        [MaxLength(50, ErrorMessage = "Household name must be 1-50 characters"), MinLength(1)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250, ErrorMessage = "Household description must be less than 250 characters")]
        public string? Description { get; set; }

        public int OwnerId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser Owner { get; set; }

        [JsonIgnore]
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        [JsonIgnore]
        public virtual ICollection<Chore> Chores { get; set; } = new List<Chore>();
    }
}
