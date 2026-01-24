namespace ChoreBuddies.Frontend.Features.Chores;

public static class ChoresConstants
{
    public static string ApiEndpointCreateChore = "api/v1/chores/add";
    public static string ApiEndpointGetChoreById = "/api/v1/chores/";
    public static string ApiEndpointGetUsersChores = "/api/v1/chores";
    public static string ApiEndpointGetHouseholdChores = "/api/v1/chores/householdChores";
    public static string ApiEndpointGetHouseholdUnverifiedChores = "/api/v1/chores/householdChores/unverified";
    public static string ApiEndpointUpdateChore = "/api/v1/chores/update";
    public static string ApiEndpointMarkChoreAsDone = "/api/v1/chores/markAsDone?choreId=";
    public static string ApiEndpointVerifyChore = "/api/v1/chores/verify?choreId=";
}
