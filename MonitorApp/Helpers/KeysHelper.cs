using System.Collections.Generic;

namespace MonitorApp.Helpers;

public static class KeysHelper
{
    private static readonly List<string> Keys = new()
    {
        "f5456978457ab9e3bae492e3e93cf68d3f587924", //Fuck You!
        "6d1de3e5742913752fe11b8e909ca021f511840c", //With love from Rao
        "5082581e74ea5492ad622914588d61e6375714a9", //Buy me a coffee!
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