namespace ChoreBuddies.Backend.Features.Notifications.Exceptions;

public class FcmTokenUnregisteredException(int userId) : Exception($"FCM token is unregistered: {userId}");
