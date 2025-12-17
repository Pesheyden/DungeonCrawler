using System;
using System.Collections;
using System.Collections.Generic;
using Feature.Inventory;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class InventoryController
{
    private readonly InventoryViewer _view;
    private readonly InventoryModel _model;
    public readonly int Capacity;


    InventoryController(InventoryViewer view, InventoryModel model, int capacity) {
        Debug.Assert(view != null, "View is null");
        Debug.Assert(model != null, "Model is null");
        Debug.Assert(capacity > 0, "Capacity is less than 1");
        _view = view;
        _model = model;
        Capacity = capacity;

        _model.Items.AnyValueChanged += HandleModelChanged;

        Initialize();

        RefreshView();
    }

    private void RefreshView()
    {
        for (int i = 0; i < Capacity; i++)
        {
            var item = _model.Get(i);
            if (item == null || item.ItemData.Id.Equals(SerializableGuid.Empty)) {
                _view.Slots[i].Set(SerializableGuid.Empty, 0);
            } else {
                _view.Slots[i].Set(item.ItemData.Id, item.Quantity);
            }
        }
    }
    
    void HandleModelChanged(IList<Item> items) => RefreshView();

    private void Initialize()
    {
        _view.Initialize(this);
    }
    
    public void HandleMove(Slot originalSlot, Slot closestSlot) {
        // Moving to Same Slot or Empty Slot
        if (originalSlot.Index == closestSlot.Index || closestSlot.ItemId.Equals(SerializableGuid.Empty)) {
            _model.Swap(originalSlot.Index, closestSlot.Index);
            return;
        }
        
        // TODO world drops
        // TODO Cross Inventory drops
        // TODO Hotbar drops
            
        // Moving to Non-Empty Slot
        var sourceItemData = _model.Get(originalSlot.Index).ItemData;
        var targetItemData = _model.Get(closestSlot.Index).ItemData;
                        
        if (sourceItemData.Id.Equals(targetItemData.Id) && 
            _model.Get(closestSlot.Index).ItemData.MaxStackSize >= _model.Get(originalSlot.Index).Quantity + _model.Get(closestSlot.Index).Quantity) 
        { 
            _model.Combine(originalSlot.Index, closestSlot.Index);
        } 
        else 
        {
            _model.Swap(originalSlot.Index, closestSlot.Index);
        }
    }

    public void AddItem(SerializableGuid id, int quantity = 1)
    {
        _model.Add(ItemsDataBase.GetDetailsById(id).Create(quantity));
    }

    public class Builder
    {
        private InventoryViewer _view;
        private IEnumerable<ItemData> _itemDetails;
        private int _capacity = 20;
            
        public Builder(InventoryViewer view) {
            _view = view;
        }

        public Builder WithStartingItems(IEnumerable<ItemData> itemDetails) {
            _itemDetails = itemDetails;
            return this;
        }

        public Builder WithCapacity(int capacity) {
            _capacity = capacity;
            return this;
        }

        public InventoryController Build() {
            InventoryModel model = _itemDetails != null 
                ? new InventoryModel(_itemDetails, _capacity) 
                : new InventoryModel(_capacity);

            return new InventoryController(_view, model, _capacity);
        }
    }
    

}
