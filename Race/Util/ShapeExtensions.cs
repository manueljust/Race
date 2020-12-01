using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Race.Util
{
    public static class ShapeExtensions
    {
        public static void ApplyStyle(this Shape shape, Dictionary<string, string> style, Dictionary<string, object> definitions = null)
        {
            foreach (KeyValuePair<string, string> pair in style)
            {
                switch (pair.Key.ToLower())
                {
                    case "fill":
                        shape.Fill = Parser.ParseBrush(pair.Value, definitions);
                        break;
                    case "stroke":
                        shape.Stroke = Parser.ParseBrush(pair.Value);
                        break;
                    case "stroke-width":
                        shape.StrokeThickness = Parser.ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-linecap":
                        shape.StrokeStartLineCap = Parser.ParseLineCap(pair.Value);
                        shape.StrokeEndLineCap = Parser.ParseLineCap(pair.Value);
                        break;
                    case "stroke-linejoin":
                        shape.StrokeLineJoin = Parser.ParseLineJoin(pair.Value);
                        break;
                    case "stroke-miterlimit":
                        shape.StrokeMiterLimit = Parser.ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-dasharray":
                        shape.StrokeDashArray = Parser.ParseDashArray(pair.Value);
                        break;
                    case "stroke-dashoffset":
                        shape.StrokeDashOffset = Parser.ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "visibility":
                        switch (pair.Value)
                        {
                            case "hidden":
                                shape.Visibility = Visibility.Hidden;
                                break;
                            case "collapsed":
                                shape.Visibility = Visibility.Collapsed;
                                break;
                        }
                        break;
                    case "opacity":
                        shape.Opacity = double.Parse(pair.Value);
                        break;
                }
            }
        }

        public static bool CollidesWith(this Shape shape, Line line)
        {
            if (shape is Line line2)
            {
                return Intersect(line, line2);
            }

            Geometry transformedGeometry = Geometry.Combine(Geometry.Empty, shape.RenderedGeometry, GeometryCombineMode.Union, shape.RenderTransform);
            if (0 != transformedGeometry.GetArea())
            {
                IntersectionDetail collision = transformedGeometry.FillContainsWithDetail(line.RenderedGeometry);

                if (collision != IntersectionDetail.Empty)
                {
                    return true;
                }
            }
            else
            {
                PathFigure pf = PathGeometry.CreateFromGeometry(transformedGeometry).Figures.FirstOrDefault();
                if (null != pf)
                {
                    Line lineRepresentation = new Line()
                    {
                        X1 = pf.StartPoint.X,
                        Y1 = pf.StartPoint.Y,
                        X2 = pf.EndPoint().X,
                        Y2 = pf.EndPoint().Y,
                    };
                    if (Intersect(line, lineRepresentation))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool Intersect(Line line1, Line line2)
        {
            // https://stackoverflow.com/questions/385305/efficient-maths-algorithm-to-calculate-intersections

            double dx1 = line1.X2 - line1.X1;
            double a1 = (line1.Y2 - line1.Y1) / (0.0 == dx1 ? 1e-15 : dx1);
            double b1 = line1.Y1 - a1 * line1.X1;

            double dx2 = line2.X2 - line2.X1;
            double a2 = (line2.Y2 - line2.Y1) / (0.0 == dx2 ? 1e-15 : dx2);
            double b2 = line2.Y1 - a2 * line2.X1;

            int s1 = Math.Sign(line2.Y1 - a1 * line2.X1 - b1);
            int s2 = Math.Sign(line2.Y2 - a1 * line2.X2 - b1);
            int s3 = Math.Sign(line1.Y1 - a2 * line1.X1 - b2);
            int s4 = Math.Sign(line1.Y2 - a2 * line1.X2 - b2);

            return (s1 != s2) && (s3 != s4);
        }
    }
}
