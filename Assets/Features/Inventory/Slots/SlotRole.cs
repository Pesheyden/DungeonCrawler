namespace Feature.Inventory
{
    public abstract class SlotRole
    {
        protected Slot slot;

        internal void Attach(Slot slot)
        {
            this.slot = slot;
            OnAttached();
        }

        protected virtual void OnAttached() { }

        public Slot Build() => slot.Build();

        public T AddRole<T>() where T : SlotRole, new()
            => slot.AddRole<T>();
    }
}