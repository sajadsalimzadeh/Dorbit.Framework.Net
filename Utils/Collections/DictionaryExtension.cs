namespace Dorbit.Utils.Collections;

public static class DictionaryExtension
{
    public static bool Replace<T,TR>(this Dictionary<T,TR> dict, T key, TR replacement)
    {
        if (!dict.ContainsKey(key)) return false;
        var obj = dict[key];
        var type = replacement.GetType();
        foreach (var prop in type.GetProperties())
        {
            if(prop.GetSetMethod() != null && prop.GetGetMethod() != null)
                prop.SetValue(obj, prop.GetValue(replacement));
        }
        return true;
    }
    public static void ReplaceOrAdd<T, TR>(this Dictionary<T, TR> dict, T key, TR replacement)
    {
        if (dict.ContainsKey(key)) dict.Replace(key, replacement);
        else dict[key] = replacement;
    }
    public static bool Replace<T, TR>(this SortedDictionary<T, TR> dict, T key, TR replacement)
    {
        if (!dict.ContainsKey(key)) return false;
        var obj = dict[key];
        var type = replacement.GetType();
        foreach (var prop in type.GetProperties())
        {
            if (prop.GetSetMethod() != null && prop.GetGetMethod() != null)
                prop.SetValue(obj, prop.GetValue(replacement));
        }
        return true;
    }
    public static void ReplaceOrAdd<T, TR>(this SortedDictionary<T, TR> dict, T key, TR replacement)
    {
        if (dict.ContainsKey(key)) dict.Replace(key, replacement);
        else dict[key] = replacement;
    }
}