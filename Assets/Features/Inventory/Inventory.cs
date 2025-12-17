using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private InventoryViewer _inventoryViewer;
    [SerializeField] private int _capacity;
    [SerializeField] private List<ItemData> _startingItems;

    public InventoryController Controller; 
    
    private void Awake()
    {
        Controller = new InventoryController.Builder(_inventoryViewer)
            .WithStartingItems(_startingItems)
            .WithCapacity(_capacity)
            .Build();
    }

    public void OpenClose()
    {
        _inventoryViewer.ChangeViewVisibility();
    }
}
