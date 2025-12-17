using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ConsoleCommandRegistry
{
    private static readonly Dictionary<string, List<MethodInfo>> commands = new();


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void UpdateCommands()
    {
        commands.Clear();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (!method.IsDefined(typeof(ConsoleCommandAttribute), false)) continue;
                    var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
                    
                    if (!commands.TryGetValue(attr.ConsoleCommand.CallName, out var list))
                    {
                        list = new List<MethodInfo>();
                        commands[attr.ConsoleCommand.CallName] = list;
                    }
                    list.Add(method);
                }
            }
        }
        
        Debug.Log("Commands updated after assembly reload.");
    }

    public static bool TryExecute(string key, string[] args, out string executionMessage)
    {
        executionMessage = default;
        if (!commands.TryGetValue(key, out var methods))
        {
            executionMessage = "<color=red>Command does not exist</color>";
            return false;
        }

        foreach (var method in methods.Where(method => method.GetParameters().Length == args.Length))
        {
            try
            {
                object[] parsedArgs = InGameConsoleUtils.ParseArguments(method, args);
                method.Invoke(null, parsedArgs);
                executionMessage = "Command executed successfully";
                return true;
            }
            catch (Exception ex)
            {
                executionMessage = $"<color=red>Command execution failed: {ex.Message}</color>";
            }
        }

        executionMessage = executionMessage ?? $"No matching overload found for command '{key}' with {args?.Length ?? 0} arguments.";
        return false;
    }

    public static List<string> GetCommandNames() => commands.Keys.ToList();
    
    
    public static List<ConsoleCommand> GetCommandInfos()
    {
        return commands.SelectMany(pair =>
            pair.Value.Select(method =>
            {
                var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
                return attr.ConsoleCommand;
            })).ToList();
    }


}
