using System;
using UnityEngine;

[Serializable]
public class ConsoleCommand
{
    public readonly string CallName;
    public readonly string Description;
    public readonly string ParametersHint;

    public ConsoleCommand(string callName, string description, string parametersHint)
    {
        CallName = callName;
        Description = description;
        ParametersHint = parametersHint;
    }
    
    
}
