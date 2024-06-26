using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace D8.Core.Extensions;

public static class EnumExtensions
{
    public static string? GetDescription(this Enum enumValue)
    {
        return enumValue.GetType().GetMember(enumValue.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetDescription();
    }

    public static IDictionary<T, string> GetEnumDescriptionList<T>(IEnumerable<T>? exclusionList = null) where T : struct, Enum
    {
        var list = new Dictionary<T, string>();

        foreach (T item in Enum.GetValues(typeof(T)))
        {
            var description = item.GetDescription();

            if (string.IsNullOrEmpty(description))
                continue;

            if (exclusionList != null && exclusionList.Contains(item))
                continue;

            list.Add(item, description);
        }

        return list;
    }

    public static IEnumerable<T> GetEnumList<T>(IEnumerable<T>? exclusionList = null) where T : struct, Enum
    {
        var list = new List<T>();

        foreach (T item in Enum.GetValues(typeof(T)))
        {
            if (exclusionList != null && exclusionList.Contains(item))
                continue;

            list.Add(item);
        }

        return list;
    }


}

