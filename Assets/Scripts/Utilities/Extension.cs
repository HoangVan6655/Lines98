using System;
using System.Collections.Generic;
using System.Linq;

public static class Extension
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source == null || !source.Any();
    }

    public static bool IsNull<T>(this IEnumerable<T> source)
    {
        return source == null;
    }

    public static bool EquivalentTo(this string source, string target)
    {
        return source.Equals(target, StringComparison.OrdinalIgnoreCase);
    }
}
