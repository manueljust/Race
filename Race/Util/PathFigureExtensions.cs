using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Race.Util
{
    public static class PathFigureExtensions
    {
        public static Point EndPoint(this PathFigure pathFigure)
        {
            PathSegment last = pathFigure.Segments.Last();
            if (last is ArcSegment)
            {
                return ((ArcSegment)last).Point;
            }
            if (last is BezierSegment)
            {
                return ((BezierSegment)last).Point3;
            }
            if (last is LineSegment)
            {
                return ((LineSegment)last).Point;
            }
            if (last is PolyBezierSegment)
            {
                return ((PolyBezierSegment)last).Points.Last();
            }
            if (last is PolyLineSegment)
            {
                return ((PolyLineSegment)last).Points.Last();
            }
            if (last is PolyQuadraticBezierSegment)
            {
                return ((PolyQuadraticBezierSegment)last).Points.Last();
            }
            if (last is QuadraticBezierSegment)
            {
                return ((QuadraticBezierSegment)last).Point2;
            }
            return new Point();
        }
    }
}
