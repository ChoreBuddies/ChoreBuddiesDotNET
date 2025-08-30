using Microsoft.AspNetCore.Identity;

namespace ChoreBuddies.Backend.Domain
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int? HouseholdId { get; set; }

        // Navigation properties
        public Household? Household { get; set; }
    }
}