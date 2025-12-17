using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Feature.Inventory
{
    public class DragRole : SlotRole
    {
        public event Action<Vector2, Slot> OnStartDrag = delegate { };

        protected override void OnAttached()
        {
            base.OnAttached();
            slot.RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || slot.ItemId.Equals(SerializableGuid.Empty)) return;
            
            OnStartDrag.Invoke(evt.position, slot);
            evt.StopPropagation();
        }

        public DragRole AddDragCallbackListener(Action<Vector2, Slot> onDrag)
        {
            OnStartDrag += onDrag;
            return this;
        }
    }
}