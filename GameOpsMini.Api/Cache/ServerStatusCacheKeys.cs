namespace GameOpsMini.Api.Cache;

public static class ServerStatusCacheKeys
{
    public static string GetServerStatusKey(int serverId)
    {
        return $"server-status:{serverId}";
    }
}