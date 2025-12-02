using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public static class InGameConsoleUtils
{
    // Registry of custom parsers for non-basic types
    private static readonly Dictionary<Type, Func<string, object>> customParsers = new()
    {
        { typeof(Vector3), s => {
            var parts = s.Split(',');
            if (parts.Length != 3) throw new FormatException("Vector3 requires 3 comma-separated values");
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }},
        { typeof(Color), s => ColorUtility.TryParseHtmlString(s, out var c) ? c : throw new FormatException("Invalid color format") },
        { typeof(bool), s => bool.Parse(s) }
        // Add more custom parsers here as needed
    };

    /// <summary>
    /// Parses string arguments into typed objects matching the method parameters.
    /// </summary>
    public static object[] ParseArguments(MethodInfo method, string[] args)
    {
        var parameters = method.GetParameters();

        if (args == null) args = Array.Empty<string>();

        if (parameters.Length != args.Length)
            throw new ArgumentException($"Expected {parameters.Length} arguments, got {args.Length}");

        var parsed = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var argString = args[i];

            if (customParsers.TryGetValue(paramType, out var parser))
            {
                parsed[i] = parser(argString);
            }
            else
            {
                parsed[i] = Convert.ChangeType(argString, paramType);
            }
        }

        return parsed;
    }
    
    public static void ValidateParameterTypes(MethodInfo methodInfo, object[] parameters)
    {
        var methodParams = methodInfo.GetParameters();

        if (methodParams.Length != parameters.Length)
            throw new ArgumentException($"Expected {methodParams.Length} parameters but got {parameters.Length}");

        for (int i = 0; i < methodParams.Length; i++)
        {
            var expected = methodParams[i].ParameterType;
            var actual = parameters[i]?.GetType();

            if (actual != null && !expected.IsAssignableFrom(actual))
            {
                throw new ArgumentException(
                    $"Parameter {i + 1} type mismatch. Expected: {expected}, Got: {actual}");
            }
        }
    }

    public static bool CompareParameters(MethodInfo methodInfo, object[] parameters)
    {
        var methodParams = methodInfo.GetParameters();
        
        if (parameters == null) return methodParams.Length == 0;
            
        if (methodParams.Length != parameters.Length)
            return false;

        for (int i = 0; i < methodParams.Length; i++)
        {
            var expected = methodParams[i].ParameterType;
            var actual = parameters[i]?.GetType();

            if (actual == null || expected.IsAssignableFrom(actual) && !ReferenceEquals(expected, actual))
            {
                return false;
            }
        }

        return true;
    }
    
    public static VisualElement GetRootVisualElement(VisualElement element)
    {
        while (element.parent != null)
            element = element.parent;
        return element;
    }

}