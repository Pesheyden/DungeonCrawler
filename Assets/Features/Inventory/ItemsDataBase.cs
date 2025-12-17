using System.Collections.Generic;
using UnityEngine;

public static class ItemsDataBase
{
    static Dictionary<SerializableGuid, ItemData> _itemDetailsDictionary;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void Initialize() {
        _itemDetailsDictionary = new Dictionary<SerializableGuid, ItemData>();
        var itemDetails = Resources.LoadAll<ItemData>("");
        foreach (var item in itemDetails) {
            _itemDetailsDictionary.Add(item.Id, item);
        }
        Debug.Log($"Cached {itemDetails.Length} items");
    }

    public static ItemData GetDetailsById(SerializableGuid id) {
        try {
            return _itemDetailsDictionary[id];
        } catch {
            Debug.LogError($"Cannot find item details with id {id}");
            return null;
        }
    }
}
