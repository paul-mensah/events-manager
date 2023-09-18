namespace EventsManager.API.Extensions;

public static class RedisConstants
{
    private const string UserKeyByUsername = "eventmanager:invitations:{username}";

    public static string GetUserInvitationsRedisKeyByUsername(string username)
    {
        return UserKeyByUsername.Replace("{username}", username);
    }
}