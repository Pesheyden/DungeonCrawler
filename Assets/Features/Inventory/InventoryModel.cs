using System.Collections.Generic;
using UnityEngine;

public class InventoryModel
{
    public readonly ObservableArray<Item> Items;
    public int Capacity;

    public InventoryModel(int capacity)
    {
        this.Capacity = capacity;
        Items = new ObservableArray<Item>(capacity);
    }

    public InventoryModel (IEnumerable<ItemData> itemData,int capacity)
    {
        this.Capacity = capacity;
        Items = new ObservableArray<Item>(capacity);
        foreach (var itemDetail in itemData) {
            Items.TryAdd(itemDetail.Create(1));
        }
    }
    
    public Item Get(int index) => Items[index];
    public void Clear() => Items.Clear();
    public bool Add(Item item) => Items.TryAdd(item);
    public bool Remove(Item item) => Items.TryRemove(item);
        
    public void Swap(int source, int target) => Items.Swap(source, target);
        
    public int Combine(int source, int target) {
        var total = Items[source].Quantity + Items[target].Quantity;
        Items[target].Quantity = total;
        Remove(Items[source]);
        return total;
    }
}
