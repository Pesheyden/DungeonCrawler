using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Feature.Inventory
{
    public class Slot : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Slot, UxmlTraits> { }

        private readonly Dictionary<Type, SlotRole> _roles = new();

        public VisualElement Icon;
        public Label StackLabel;
        protected ItemData ItemData;
        public int Index => parent.IndexOf(this);
        public SerializableGuid ItemId { get; protected set; } = SerializableGuid.Empty;
        public Sprite BaseSprite;

        public void Set(SerializableGuid id, int qty = 0) 
        {
            ItemId = id;
            if (ItemId == SerializableGuid.Empty)
            {
                Remove();
                return;
            }

            ItemData = ItemsDataBase.GetDetailsById(id);
            BaseSprite = ItemData.Icon;
            
            Icon.style.backgroundImage = BaseSprite != null ? BaseSprite.texture : null;
            StackLabel.text = qty > 1 ? qty.ToString() : string.Empty;
            StackLabel.visible = qty > 1;
        }

        public void Remove() 
        {
            ItemId = SerializableGuid.Empty;
            Icon.style.backgroundImage = null;
            StackLabel.text = string.Empty;
        }
        
        public virtual Slot Build()
        {
            Icon = this.Q("Icon");
            StackLabel = this.Q<Label>("Stack_Label");;
            return this;
        }

        public T AddRole<T>() where T : SlotRole, new()
        {
            var role = new T();
            role.Attach(this);
            _roles[typeof(T)] = role;
            return role;
        }

        public bool TryGetRole<T>(out T role) where T : SlotRole
        {
            if (_roles.TryGetValue(typeof(T), out var r))
            {
                role = (T)r;
                return true;
            }

            role = null;
            return false;
        }

    }
}