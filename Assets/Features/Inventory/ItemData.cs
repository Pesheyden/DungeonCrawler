using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public SerializableGuid Id;
    public Sprite Icon;
    public string Name;
    public int MaxStackSize;
    [TextArea] public string Description;
    public Item Create(int quantity) {
        return new Item(this, quantity);
    }
}
