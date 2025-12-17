using System.Collections.Generic;
using UnityEngine;

public class InventoriesConsoleHub : MonoBehaviour
{
    private static readonly Dictionary<string, Inventory> _inventoriesDic = new Dictionary<string, Inventory>();
    [SerializeField] private List<Inventory> _inventories;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var inventory in _inventories)
        {
            _inventoriesDic.Add(inventory.name,inventory);
        }
    }

    [ConsoleCommand("add_item", "inventoryName itemId", "Adds an item by id to a certain inventory")]
    public static void AddItem(string inventoryKey, string itemId)
    {
        if(!_inventoriesDic.TryGetValue(inventoryKey, out var inventory))
            return;
        inventory.Controller.AddItem(SerializableGuid.FromHexString(itemId));
    }
    [ConsoleCommand("add_item", "inventoryName itemId quantity", "Adds certain amount of one item by id to a certain inventory")]
    public static void AddItem(string inventoryKey, string itemId, int quantity)
    {
        if(!_inventoriesDic.TryGetValue(inventoryKey, out var inventory))
            return;
        inventory.Controller.AddItem(SerializableGuid.FromHexString(itemId), quantity);
    }
}
