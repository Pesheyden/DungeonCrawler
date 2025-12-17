using System;
using System.Linq;
using Feature.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventoryViewer : InventoryViewer
{
    [SerializeField] private VisualTreeAsset _ghostSlotAsset;
    protected Slot GhostIcon;

    private bool _isDragging;
    private Slot _originalSlot;
    
    protected override Slot SlotFactory()
    {
        return SlotAsset.CloneTree().Q<Slot>().AddRole<DragRole>().AddDragCallbackListener(OnPointerDown).Build();
    }

    public override void Initialize(InventoryController controller)
    {
        base.Initialize(controller);

        GhostIcon = _ghostSlotAsset.CloneTree().Q<Slot>().Build();
        GhostIcon.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        GhostIcon.RegisterCallback<PointerUpEvent>(OnPointerUp);
        GhostIcon.style.visibility = Visibility.Hidden;
        Root.Add(GhostIcon);
    }

    private void OnPointerDown(Vector2 pos, Slot slot)
    {
        _isDragging = true;
        _originalSlot = slot;
        
        SetGhostIconPosition(pos);

        GhostIcon.Set(slot.ItemId);
        _originalSlot.Icon.style.backgroundImage = null;
        _originalSlot.StackLabel.visible = false;
            
        GhostIcon.style.visibility = Visibility.Visible;
        GhostIcon.StackLabel.text = _originalSlot.StackLabel.text;
        GhostIcon.StackLabel.visible = _originalSlot.StackLabel.text != string.Empty;
    }
    void OnPointerMove(PointerMoveEvent evt) {
        if (!_isDragging) return;
            
        SetGhostIconPosition(evt.position);
    }

    void OnPointerUp(PointerUpEvent evt) {
        if (!_isDragging) return;
        Slot closestSlot = Slots
            .Where(slot => slot.worldBound.Overlaps(GhostIcon.worldBound))
            .OrderBy(slot => Vector2.Distance(slot.worldBound.position, GhostIcon.worldBound.position))
            .FirstOrDefault();
        
        _originalSlot.Remove();
        if (closestSlot != null)
        {
            Controller.HandleMove(_originalSlot, closestSlot);
        } 
        else 
        {
            Controller.HandleMove(_originalSlot, _originalSlot);
        }
        _originalSlot.StackLabel.visible = true;

            
        _isDragging = false;
        _originalSlot = null;
        GhostIcon.style.visibility = Visibility.Hidden;
        GhostIcon.StackLabel.visible = false;
    }
    

    private void SetGhostIconPosition(Vector2 position) {
        GhostIcon.style.top = position.y - GhostIcon.layout.height / 2;
        GhostIcon.style.left = position.x - GhostIcon.layout.width / 2;
    }
}
