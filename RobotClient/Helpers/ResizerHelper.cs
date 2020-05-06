using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace RobotInterface.Helpers
{
    public class ResizerHelper
    {
        public HitType MouseHitType { get; set; }

        public ResizerHelper()
        {
            MouseHitType = HitType.None;
        }

        public enum HitType
        {
            None,
            Body,
            Left,
            Right
        };

        private bool DragInProgress = false;

        private Point LastPoint;
        private object rectangle1;

        public HitType SetHitType(Rectangle rect, Point point)
        {
            double left = Canvas.GetLeft(rect);
            double right = left + rect.Width;
            if (point.X < left) return HitType.None;
            if (point.X > right) return HitType.None;

            const double GAP = 10;
            if (point.X - left < GAP)
            {
                // Left edge.
                return HitType.Left;
            }
            else if (right - point.X < GAP)
            {
                // Right edge.
                return HitType.Right;
            }

            return HitType.Body;
        }

        //Set a mouse cursor appropriate for the current hit type.
        public Cursor SetMouseCursor()
        {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Arrow;
            switch (MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.ScrollAll;
                    break;
                case HitType.Left:
                case HitType.Right:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }

            // Display the desired cursor.
            return desired_cursor;
        }
    }
}
