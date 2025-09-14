using Microsoft.AspNetCore.Identity;

namespace ChoreBuddies.Backend.Domain;

public class AppUser : IdentityUser<int>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public int? HouseholdId { get; set; }

    // Refresh Token Properties
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }

    // Navigation properties
    public Household? Household { get; set; }
}
