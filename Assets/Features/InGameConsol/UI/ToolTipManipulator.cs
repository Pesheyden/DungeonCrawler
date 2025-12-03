using UnityEngine;
using UnityEngine.UIElements;

public class ToolTipManipulator : Manipulator
{
    private VisualElement _element;
    private VisualTreeAsset _visualTreeAsset;

    public ToolTipManipulator(VisualTreeAsset tooltipTemplate)
    {
        _visualTreeAsset = tooltipTemplate;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(MouseIn);
        target.RegisterCallback<MouseOutEvent>(MouseOut);
        target.RegisterCallback<DetachFromPanelEvent>(OnDetach);
    }
    

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(MouseIn);
        target.UnregisterCallback<MouseOutEvent>(MouseOut);
        target.RegisterCallback<DetachFromPanelEvent>(OnDetach);
    }

    private void MouseIn(MouseEnterEvent e)
    {
        if(target.tooltip == null)
            return;
        if (_element == null)
        {
            _element = _visualTreeAsset.CloneTree();
            _element.Q<Label>().text = target.tooltip;
            _element.style.left = this.target.worldBound.center.x;
            _element.style.top = this.target.worldBound.yMin;
            var root = InGameConsoleUtils.GetRootVisualElement(this.target);
            root.Add(_element);
        }

        _element.style.visibility = Visibility.Visible;
        _element.BringToFront();
    }

    private void MouseOut(MouseOutEvent e)
    {
        if(target.tooltip == null)
            return;
        _element.style.visibility = Visibility.Hidden;
    }
    private void OnDetach(DetachFromPanelEvent evt)
    {
        _element?.RemoveFromHierarchy();
    }
}