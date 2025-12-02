using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ConsoleCommandAttribute : Attribute
{
    public readonly ConsoleCommand ConsoleCommand;

    public ConsoleCommandAttribute(string name, string parametersHint = "", string description = "")
    {
        ConsoleCommand = new ConsoleCommand(name, description, parametersHint);
    }

}
