using System.Windows;
using System.Windows.Shapes;

namespace Race.Util
{
    public static class LineExtensions
    {
        public static Point StartPoint(this Line line)
        {
            return new Point(line.X1, line.Y1);
        }

        public static Vector Vector(this Line line)
        {
            return new Vector(line.X2 - line.X1, line.Y2 - line.Y1);
        }
    }
}
