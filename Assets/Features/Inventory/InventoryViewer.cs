using System;
using System.Collections.Generic;
using Feature.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryViewer : MonoBehaviour
{
    [SerializeField] protected UIDocument Document;
    [SerializeField] protected VisualTreeAsset BaseAsset;
    [SerializeField] protected VisualTreeAsset SlotAsset;
    
    protected InventoryController Controller;
    protected VisualElement Root;
    protected VisualElement SlotsRoot;
    public List<Slot> Slots;

    /// <summary>
    /// Factory that determines what concrete Slot type is created.
    /// Default: plain Slot. Override or assign in derived UIs.
    /// </summary>
    protected virtual Slot SlotFactory()
    {
        return SlotAsset.CloneTree().Q<Slot>().Build();
    }

    public virtual void Initialize(InventoryController controller)
    {
        Controller = controller;
        
        
        Document ??= GetComponent<UIDocument>();
        Root = Document.rootVisualElement;
        Root.Add(BaseAsset.CloneTree());
        
        SlotsRoot = Root.Q("SlotsRoot");
        if (SlotsRoot == null)
        {
            Debug.LogError($"InventoryViewer {name}: 'SlotsRoot' element not found in UXML: {BaseAsset.name}.");
            return;
        }

        Slots = new List<Slot>();
        for (int i = 0; i < Controller.Capacity; i++)
        {
            var slot = SlotFactory();
            SlotsRoot.Add(slot);

            Slots.Add(slot);
        }
    }
}
