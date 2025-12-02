using UnityEditor;

[InitializeOnLoad]
public class ConsoleCommandUpdater
{
    static ConsoleCommandUpdater()
    {
        AssemblyReloadEvents.afterAssemblyReload += () =>
        {
            ConsoleCommandRegistry.UpdateCommands();
            UnityEngine.Debug.Log("Commands updated after assembly reload.");
        };
    }
}