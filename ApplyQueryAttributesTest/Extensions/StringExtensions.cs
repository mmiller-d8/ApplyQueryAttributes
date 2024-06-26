using System;
namespace D8.Core.Extensions;

public static class StringExtensions
{
	public static string ReplacePlaceholder(this string target, string replaceWith, string? variable = null, string variableStartCharacters = "{{", string variableEndCharacters = "}}")
	{
        var variableStart = target.IndexOf(variableStartCharacters);
        var variableEnd = target.IndexOf(variableEndCharacters);

        if (variableStart == -1 || variableEnd == -1)
            return target;

        variableEnd += variableEndCharacters.Length;

        var firstPart = target.Substring(0, variableStart);
        var lastPart = target.Substring(variableEnd, target.Length - variableEnd);

        var variableName = target.Substring(variableStart + variableStartCharacters.Length, variableEnd - variableStart - variableEndCharacters.Length - 2);

        if (variable == null || variableName.Equals(variable))
            target = $"{firstPart}{replaceWith}{lastPart}";

        return target;
    }
}

