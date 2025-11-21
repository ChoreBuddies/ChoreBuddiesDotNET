using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoreBuddies.Backend.Domain;

public class ChatMessage(int senderId, int householdId, string content, DateTimeOffset sentAt)
{
    public int Id { get; set; }

    [Required]
    [MaxLength(1000, ErrorMessage = "Wiadomość nie może przekraczać 1000 znaków.")]
    public string Content { get; set; } = content;

    public DateTimeOffset SentAt { get; set; } = sentAt;

    public int SenderId { get; set; } = senderId;

    public int HouseholdId { get; set; } = householdId;

    // Navigation properties

    [ForeignKey(nameof(SenderId))]
    public virtual AppUser? Sender { get; set; }

    [ForeignKey(nameof(HouseholdId))]
    public virtual Household? Household { get; set; }
}
