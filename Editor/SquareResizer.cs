using UnityEngine;
using UnityEngine.UIElements;

namespace Sonic853.UpmGithubManager
{
    class SquareResizer : MouseManipulator
    {
        private Vector2 m_Start;
        protected bool m_Active;
        // private MainWindow m_Splitter;
        private VisualElement m_object;
        // private static float m_float;
        // private static float m_panelfloat;
        // {
        //     get
        //     {
        //         return m_float
        //     }
        // }
        private Direction m_direction;
        private PanelFloat m_panelfloat;
        private float m_minSize;

        // public SquareResizer(MainWindow splitter)
        public SquareResizer(VisualElement _object, Direction direction = Direction.Horizontal, PanelFloat panelfloat = PanelFloat.Left, float minSize = 100)
        {
            // m_Splitter = splitter;
            m_object = _object;
            // m_panelfloat = panelfloat;
            m_direction = direction;
            m_panelfloat = panelfloat;
            m_minSize = minSize;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            m_Active = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                m_Start = e.localMousePosition;

                m_Active = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active || !target.HasMouseCapture())
                return;

            Vector2 diff = e.localMousePosition - m_Start;

            if (m_direction == Direction.Horizontal)
            {
                if (m_panelfloat == PanelFloat.Left)
                {
                    m_object.style.width = m_object.layout.width + diff.x;
                }
                else
                {
                    m_object.style.width = m_object.layout.width - diff.x;
                }
                if (m_object.resolvedStyle.width < m_minSize)
                {
                    m_object.style.width = m_minSize;
                }
            }
            else
            {
                m_object.style.height = m_object.layout.height + diff.y;
                if (m_object.resolvedStyle.height < m_minSize)
                {
                    m_object.style.height = m_minSize;
                }
            }

            e.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent e)
        {
            if (!m_Active || !target.HasMouseCapture() || !CanStopManipulation(e))
                return;

            m_Active = false;
            target.ReleaseMouse();
            e.StopPropagation();
            if (m_direction == Direction.Horizontal)
            {
                if (m_object.resolvedStyle.width < m_minSize)
                {
                    m_object.style.width = m_minSize;
                }
            }
            else
            {
                if (m_object.resolvedStyle.height < m_minSize)
                {
                    m_object.style.height = m_minSize;
                }
            }
            // if (m_horizontal)
            // {
            //     m_panelfloat = m_object.resolvedStyle.width;
            // }
            // else
            // {
            //     m_panelfloat = m_object.resolvedStyle.height;
            // }
            // m_Splitter.SaveViewData();
        }
        public enum Direction
        {
            Horizontal = 0,
            Vertical = 1,
        }
        public enum PanelFloat
        {
            Left = 0,
            Right = 1,
        }
    }
}
