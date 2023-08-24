using System.Collections.Generic;

namespace MonitorApp.Helpers;

public static class KeysHelper
{
    private static readonly List<string> Keys = new()
    {
        "925a4697de0cbf1a1b67b9eb9c97555f0d4ab8fd",
        "72ec1ecc960a10758dafa6bebe7933fc2a7a4568",
        "05c335118971567ddb6e28519da84acda65fbff4",
    };


    public static bool IsValidKey(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            return Keys.Contains(key.Trim());
        }

        return false;
    }
}