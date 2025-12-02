using NaughtyAttributes;
using UnityEngine;

public class TestInGameConsole : MonoBehaviour
{
    [ConsoleCommand("debug")]
    public static void DebugCommand()
    {
        Debug.Log("DEBUG");
    }
    
    [ConsoleCommand("debug_log")]
    public static void DebugLogCommand()
    {
        Debug.Log("DEBUG_LOG");
    }
    
    [ConsoleCommand("debug_war")]
    public static void DebugWarCommand()
    {
        Debug.Log("DEBUG_WAR");
    }
    
    [ConsoleCommand("debug_err")]
    public static void DebugErrCommand()
    {
        Debug.Log("DEBUG_ERR");
    }
    [ConsoleCommand("spawnEnemy", "type", "Spawns an enemy of given type and count")]
    public static void SpawnEnemy(string type)
    {
        Debug.Log($"Spawning {type}");
    }
    [ConsoleCommand("spawnEnemy", "type count", "Spawns an enemy of given type and count")]
    public static void SpawnEnemy(string type, int count)
    {
        Debug.Log($"Spawning {count} {type}(s)");
    }
    [ConsoleCommand("spawnEnemy", "type count pos ", "Spawns an enemy of given type and count")]
    public static void SpawnEnemy(string type, int count, Vector3 pos)
    {
        Debug.Log($"Spawning {count} {type}(s) at {pos}");
    }
    [ConsoleCommand("spawnEnemy", "type count pos color", "Spawns an enemy of given type and count")]
    public static void SpawnEnemy(string type, int count, Vector3 pos, Color color)
    {
        Debug.Log($"Spawning {count} {color} {type}(s) at {pos}");
    }
    
}