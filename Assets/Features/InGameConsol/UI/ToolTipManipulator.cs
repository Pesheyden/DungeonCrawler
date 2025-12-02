using UnityEngine;
using UnityEngine.UIElements;

public class ToolTipManipulator : Manipulator
{
    private VisualElement element;
    private VisualTreeAsset _visualTreeAsset;

    public ToolTipManipulator(VisualTreeAsset tooltipTemplate)
    {
        _visualTreeAsset = tooltipTemplate;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(MouseIn);
        target.RegisterCallback<MouseOutEvent>(MouseOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(MouseIn);
        target.UnregisterCallback<MouseOutEvent>(MouseOut);
    }

    private void MouseIn(MouseEnterEvent e)
    {
        if(target.tooltip == null)
            return;
        if (element == null)
        {
            element = _visualTreeAsset.CloneTree();
            element.Q<Label>().text = target.tooltip;
            element.style.left = this.target.worldBound.center.x;
            element.style.top = this.target.worldBound.yMin;
            var root = InGameConsoleUtils.GetRootVisualElement(this.target);
            root.Add(element);
        }

        element.style.visibility = Visibility.Visible;
        element.BringToFront();
    }

    private void MouseOut(MouseOutEvent e)
    {
        if(target.tooltip == null)
            return;
        element.style.visibility = Visibility.Hidden;
    }
}