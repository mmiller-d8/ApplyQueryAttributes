using System;
namespace D8.Core.Extensions;

public static class DictionaryExtensions
{
    public static T? GetValue<T>(this IDictionary<string, object> attributes, string key)
    {
        attributes.TryGetValue(key, out var value);
        if (value == null)
            return (T?)default;
        else if (value is T)
            return (T)value;
        else
            throw new Exception("Found value is not of the correct type");
    }

    // public static void AddOrReplace(this IDictionary<string, object> dictionary, string key, object value)
    // {
    //     if (dictionary.ContainsKey(key))
    //         dictionary[key] = value;
    //     else
    //         dictionary.Add(key, value);
    // }

    public static void AddOrReplace<T>(this IDictionary<string, T> dictionary, string key, T value)
    {
        if (dictionary.ContainsKey(key))
            dictionary[key] = value;
        else
            dictionary.Add(key, value);
    }
}

