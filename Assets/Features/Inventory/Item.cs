using UnityEngine;

public class Item
{
    public SerializableGuid SerializableGuid;
    public ItemData ItemData;
    public int Quantity;

    public Item(ItemData itemData, int quantity)
    {
        ItemData = itemData;
        Quantity = quantity;
        SerializableGuid = SerializableGuid.NewGuid();
    }
}
