using UnityEngine.UIElements;

namespace Sonic853.UpmGithubManager
{
    class DragLine
    {
        public static VisualElement CreateDragLine(VisualElement obj, SquareResizer.Direction direction = SquareResizer.Direction.Horizontal, SquareResizer.PanelFloat panelfloat = SquareResizer.PanelFloat.Left, float minSize = 100)
        {
            // dragLineAnchor
            VisualElement dragLineAnchor = new VisualElement();
            dragLineAnchor.name = "unity-debugger-splitter-dragline-anchor";
            dragLineAnchor.AddToClassList(direction == SquareResizer.Direction.Horizontal ? "horizontal" : "vertical");
            // dragLine
            VisualElement dragLine = new VisualElement();
            dragLine.name = "unity-debugger-splitter-dragline";
            dragLine.AddManipulator(new SquareResizer(obj, direction, panelfloat, minSize));
            dragLineAnchor.Add(dragLine);
            return dragLineAnchor;
        }
    }
}
