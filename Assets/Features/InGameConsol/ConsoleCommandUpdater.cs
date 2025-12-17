#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ConsoleCommandUpdater
{
    static ConsoleCommandUpdater()
    {
        //AssemblyReloadEvents.afterAssemblyReload += ConsoleCommandRegistry.UpdateCommands;
    }
}
#endif