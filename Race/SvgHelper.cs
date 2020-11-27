using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Race
{
    public static class SvgHelper
    {
        public static Track Load(string fileName)
        {
            Track track = new Track()
            {
                Width = 210,
                Height = 150,
                Background = Brushes.ForestGreen,
            };

            XDocument xdoc = XDocument.Load(fileName);

            try
            {
                string[] nums = xdoc.Root.Attribute("viewBox").Value.Split(' ');
                track.Width = double.Parse(nums[2]) - double.Parse(nums[0]);
                track.Height = double.Parse(nums[3]) - double.Parse(nums[1]);
            }
            catch
            {
                // ignore errors, keep default width and height
            }

            Dictionary<string, object> defs = xdoc.Root.GetDefinitions();

            List<Shape> decorations = new List<Shape>();
            List<Shape> obstacles = new List<Shape>();
            foreach (XElement element in xdoc.Descendants().Where(d => "g" == d.Name.LocalName).SelectMany(g => g.Descendants()))
            {
                    switch (element.GetDescription().ToLower())
                    {
                        case "bounds":
                            if (element.TryGetPath(out Path boundsPath))
                            {
                                track.Bounds = boundsPath;
                            }
                            break;
                        case "start":
                            if (element.TryGetLine(out Line startLine))
                            {
                                track.Start = startLine;
                            }
                            break;
                        case "goal":
                            if (element.TryGetLine(out Line goalLine))
                            {
                                track.Goal = goalLine;
                            }
                            break;
                        case "obstacle":
                            if (element.TryGetShape(out Shape obastcleShape))
                            {
                                obstacles.Add(obastcleShape);
                            }
                            break;
                        default:
                            if (element.TryGetShape(out Shape decorationShape))
                            {
                                decorations.Add(decorationShape);
                            }
                            break;
                    }
            }
            track.Obstacles = obstacles.ToArray();
            track.Decorations = decorations.ToArray();

            if (null == track.Start)
            {
                throw new InvalidOperationException("No element with description \"start\" found.");
            }
            if (null == track.Goal)
            {
                // encoding and parsing seems to be the most straightforward method to deep copy
                track.Goal = (Line)System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(track.Start));
                track.Goal.Opacity = 0;
            }
            if (null == track.Bounds)
            {
                throw new InvalidOperationException("No element with description \"bounds\" found.");
            }

            return track;
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

        #region parse helpers
        public static Brush ParseBrush(string s)
        {
            try
            {
                return "none" == s.ToLower() ? null : new SolidColorBrush((Color)ColorConverter.ConvertFromString(s));
            }
            catch
            {
                return Brushes.Black;
            }
        }

        public static PenLineCap ParseLineCap(string s)
        {
            switch (s.ToLower())
            {
                case "round":
                    return PenLineCap.Round;
                case "square":
                    return PenLineCap.Square;
                default:
                    return PenLineCap.Flat;
            }
        }

        public static PenLineJoin ParseLineJoin(string s)
        {
            switch (s.ToLower())
            {
                case "bevel":
                    return PenLineJoin.Bevel;
                case "miter":
                    return PenLineJoin.Miter;
                default:
                    return PenLineJoin.Round;
            }
        }

        public static DoubleCollection ParseDashArray(string s)
        {
            return "none" == s.ToLower() ? null : new DoubleCollection(s.Split(',').Select(n => ParseDoubleIgnoreNonDigits(n)));
        }

        public static FontStyle ParseFontStyle(string s)
        {
            switch (s.ToLower())
            {
                case "italic":
                    return FontStyles.Italic;
                case "oblique":
                    return FontStyles.Oblique;
                default:
                    return FontStyles.Normal;
            }
        }

        public static FontWeight ParseFontWeight(string s)
        {
            switch (s.ToLower())
            {
                case "bold":
                    return FontWeights.Bold;
                case "bolder":
                    return FontWeights.ExtraBold;
                case "lighter":
                    return FontWeights.Light;
                default:
                    return FontWeights.Normal;
            }
        }

        public static double ParseDoubleIgnoreNonDigits(string s)
        {
            if(null == s)
            {
                return 0.0;
            }
            try
            {
                return double.Parse(string.Join("", s.Where(c => char.IsDigit(c) || '.' == c)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                return 0;
            }
        }
        #endregion

        #region XElement extensions
        public static IEnumerable<XElement> ElementsByLocalName(this XElement element, string localName)
        {
            return element.Elements().Where(child => localName == child.Name.LocalName);
        }

        public static double GetDouble(this XElement element, string name, double defaultValue = 0)
        {
            XAttribute att = element.Attribute(name);
            if (null != att)
            {
                return double.Parse(att.Value);
            }
            else
            {
                return defaultValue;
            }
        }

        public static Brush GetBrush(this XElement element, string name)
        {
            XAttribute att = element.Attribute(name);
            if (null != att)
            {
                return ParseBrush(att.Value);
            }
            else
            {
                return null;
            }
        }

        public static Geometry GetTextGeometry(this XElement element)
        {
            IEnumerable<Geometry> spanGeometries = element.ElementsByLocalName("tspan").Select(child => GetTextGeometry(child));

            Dictionary<string, string> style = element.GetStyle();

            if (0 == spanGeometries.Count())
            {
                return GetTextSpanGeometry(element, style);
            }
            else
            {
                Geometry baseGeometry = Geometry.Empty;
                foreach(Geometry spanGeometry in spanGeometries)
                {
                    baseGeometry = new CombinedGeometry(baseGeometry, spanGeometry);
                }
                baseGeometry.Transform = element.GetTransform();
                return baseGeometry;
            }
        }

        public static Geometry GetTextSpanGeometry(this XElement element, Dictionary<string, string> baseStyle )
        {
            Dictionary<string, string> style = element.GetStyle(baseStyle);

            double fontSize = ParseDoubleIgnoreNonDigits(style["font-size"]);

            FormattedText text = new FormattedText(
                element.Value,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily(style["font-family"]),
                    ParseFontStyle(style["font-style"]),
                    ParseFontWeight(style["font-weight"]),
                    FontStretches.Normal),
                fontSize,
                ParseBrush(style["fill"])
                );

            Geometry geometry = text.BuildGeometry(new Point(element.GetDouble("x"), element.GetDouble("y") - fontSize));
            geometry.Transform = element.GetTransform();

            return geometry;
        }

        public static string GetDescription(this XElement element)
        {
            return element.ElementsByLocalName("desc").FirstOrDefault()?.Value ?? string.Empty;
        }

        public static Dictionary<string, string> GetStyle(this XElement element, Dictionary<string, string> baseStyle = null)
        {
            Dictionary<string, string> style = baseStyle ?? new Dictionary<string, string>()
            {
                { "font-family", "sans-serif" },
                { "font-style", "normal" },
                { "font-weight", "normal" },
                { "font-size", "12" },
                { "fill", "#000000" },
            };

            XAttribute styleElement = element.Attribute("style");
            if(null != styleElement)
            {
                string[] styleStrings = styleElement.Value.Split(';');
                foreach(string styleString in styleStrings)
                {
                    string[] keyValueStrings = styleString.Split(':');
                    if(2 == keyValueStrings.Length)
                    {
                        style[keyValueStrings[0]] = keyValueStrings[1];
                    }
                }
            }

            return style;
        }

        public static bool TryGetShape(this XElement element, out Shape shape)
        {
            switch (element.Name.LocalName.ToLower())
            {
                case "circle":
                    if(element.TryGetCircle(out Ellipse circle))
                    {
                        shape = circle;
                        return true;
                    }
                    break;
                case "ellipse":
                    if(element.TryGetEllipse(out Ellipse ellipse))
                    {
                        shape = ellipse;
                        return true;
                    }
                    break;
                case "line":
                    if(element.TryGetLine(out Line line))
                    {
                        shape = line;
                        return true;
                    }
                    break;
                case "path":
                    if (element.TryGetPath(out Path path))
                    {
                        shape = path;
                        return true;
                    }
                    break;
                case "polygon":
                    if (element.TryGetPolygon(out Polygon polygon))
                    {
                        shape = polygon;
                        return true;
                    }
                    break;
                case "polyline":
                    if (element.TryGetPolyline(out Polyline polyline))
                    {
                        shape = polyline;
                        return true;
                    }
                    break;
                case "rect":
                    if (element.TryGetRectangle(out Rectangle rectangle))
                    {
                        shape = rectangle;
                        return true;
                    }
                    break;
                case "text":
                    if (element.TryGetText(out Path text))
                    {
                        shape = text;
                        return true;
                    }
                    break;
                case "image":
                    shape = null;
                    return false;
            }
            shape = null;
            return false;
        }

        public static bool TryGetCircle(this XElement element, out Ellipse ellipse)
        {
            try
            {
                double x = ParseDoubleIgnoreNonDigits(element.Attribute("cx")?.Value);
                double y = ParseDoubleIgnoreNonDigits(element.Attribute("cy")?.Value);
                double r = ParseDoubleIgnoreNonDigits(element.Attribute("r")?.Value);

                ellipse = new Ellipse()
                {
                    Fill = element.GetBrush("fill"),
                    Stroke = element.GetBrush("stroke"),
                    Height = 2 * r,
                    Width = 2 * r,
                    RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { element.GetTransform(), new TranslateTransform(x - r, y - r) }) },
                };

                ellipse.ApplyStyle(element.GetStyle());

                return true;
            }
            catch
            {
                ellipse = null;
            }
            return false;
        }

        public static bool TryGetEllipse(this XElement element, out Ellipse ellipse)
        {
            try
            {
                double x = ParseDoubleIgnoreNonDigits(element.Attribute("cx")?.Value);
                double y = ParseDoubleIgnoreNonDigits(element.Attribute("cy")?.Value);
                double rx = ParseDoubleIgnoreNonDigits(element.Attribute("rx")?.Value);
                double ry = ParseDoubleIgnoreNonDigits(element.Attribute("ry")?.Value);

                ellipse = new Ellipse()
                {
                    Fill = element.GetBrush("fill"),
                    Stroke = element.GetBrush("stroke"),
                    Height = 2 * rx,
                    Width = 2 * ry,
                    RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { element.GetTransform(), new TranslateTransform(x - rx, y - ry) }) },
                };

                ellipse.ApplyStyle(element.GetStyle());

                return true;
            }
            catch
            {
                ellipse = null;
            }
            return false;
        }

        public static bool TryGetLine(this XElement element, out Line line)
        {
            Dictionary<string, string> style = element.GetStyle();

            try
            {
                line = new Line()
                {
                    Fill = element.GetBrush("fill"),
                    Stroke = element.GetBrush("stroke"),
                };

                switch (element.Name.LocalName.ToLower())
                {
                    case "line":
                        line.X1 = element.GetDouble("x1");
                        line.Y1 = element.GetDouble("y1");
                        line.X2 = element.GetDouble("x2");
                        line.Y2 = element.GetDouble("y2");
                        line.RenderTransform = element.GetTransform();
                        line.ApplyStyle(style);
                        return true;
                    case "path":
                        FillRule fillRule = FillRule.EvenOdd;
                        Transform transform = element.GetTransform();
                        PathGeometry pg = new PathGeometry((PathFigureCollection)(new PathFigureCollectionConverter()).ConvertFromString(element.Attribute("d").Value), fillRule, transform);
                        line.X1 = pg.Figures.First().StartPoint.X;
                        line.Y1 = pg.Figures.First().StartPoint.Y;
                        line.X2 = pg.Figures.First().EndPoint().X;
                        line.Y2 = pg.Figures.First().EndPoint().Y;
                        line.ApplyStyle(style);
                        return true;
                    default:
                        line = null;
                        return false;
                }
            }
            catch
            {
                line = null;
                return false;
            }
        }

        public static bool TryGetPath(this XElement element, out Path path)
        {
            try
            {
                path = new Path()
                {
                    Fill = element.GetBrush("fill"),
                    Stroke = element.GetBrush("stroke"),
                };

                Dictionary<string, string> style = element.GetStyle();
                FillRule fillRule = style.ContainsKey("fill-rule") ? style["fill-rule"] == "nonzero" ? FillRule.Nonzero : FillRule.EvenOdd : FillRule.EvenOdd;
                Transform transform = element.GetTransform();
                path.Data = new PathGeometry((PathFigureCollection)(new PathFigureCollectionConverter()).ConvertFromString(element.Attribute("d").Value), fillRule, transform);

                path.ApplyStyle(style);
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        public static bool TryGetPolygon(this XElement element, out Polygon polygon)
        {
            try
            {
                polygon = new Polygon()
                {
                    Fill = element.GetBrush("fill"),
                    Stroke = element.GetBrush("stroke"),
                    Points = new PointCollection(element.Attribute("points").Value.Split(' ').Select(s => Point.Parse(s))),
                    RenderTransform = element.GetTransform(),
                };

                polygon.ApplyStyle(element.GetStyle());
                return true;
            }
            catch
            {
                polygon = null;
                return false;
            }
        }

        public static bool TryGetPolyline(this XElement element, out Polyline polyline)
        {
            try
            {
                polyline = new Polyline()
                {
                    Fill = element.GetBrush("fill"),
                    Stroke = element.GetBrush("stroke"),
                    Points = new PointCollection(element.Attribute("points").Value.Split(' ').Select(s => Point.Parse(s))),
                    RenderTransform = element.GetTransform(),
                };

                polyline.ApplyStyle(element.GetStyle());
                return true;
            }
            catch
            {
                polyline = null;
                return false;
            }
        }

        public static bool TryGetRectangle(this XElement element, out Rectangle rectangle)
        {
            try
            {
                double x = ParseDoubleIgnoreNonDigits(element.Attribute("x")?.Value);
                double y = ParseDoubleIgnoreNonDigits(element.Attribute("y")?.Value);
                double w = ParseDoubleIgnoreNonDigits(element.Attribute("width")?.Value);
                double h = ParseDoubleIgnoreNonDigits(element.Attribute("height")?.Value);
                double rx = ParseDoubleIgnoreNonDigits(element.Attribute("rx")?.Value);
                double ry = ParseDoubleIgnoreNonDigits(element.Attribute("ry")?.Value);

                rectangle = new Rectangle()
                {
                    Fill = element.GetBrush("fill"),
                    Stroke = element.GetBrush("stroke"),
                    Width = w,
                    Height = h,
                    RadiusX = 0.0 == rx ? ry : rx,
                    RadiusY = 0.0 == ry ? rx : ry,
                    RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { element.GetTransform(), new TranslateTransform(x, y) }) },
                };

                rectangle.ApplyStyle(element.GetStyle());
                return true;
            }
            catch
            {
                rectangle = null;
                return false;
            }
        }

        public static bool TryGetText(this XElement element, out Path text)
        {
            try
            {
                double x = ParseDoubleIgnoreNonDigits(element.Attribute("x")?.Value);
                double y = ParseDoubleIgnoreNonDigits(element.Attribute("y")?.Value);

                text = new Path()
                {
                    Data = PathGeometry.CreateFromGeometry(element.GetTextGeometry()),
                };

                text.ApplyStyle(element.GetStyle());
                return true;
            }
            catch
            {
                text = null;
                return false;
            }
        }

        public static bool TryGetGradientStop(this XElement element, out GradientStop gradientStop)
        {
            try
            {
                Dictionary<string, string> style = element.GetStyle();

                Color c = (Color)ColorConverter.ConvertFromString(style.TryGetValue("stop-color", out string color) ? color : "black");
                c.A = 0xff == c.A ? style.TryGetValue("stop-opacity", out string opacity) ? (byte)(double.Parse(opacity) * 0xff) : c.A : c.A;

                gradientStop = new GradientStop()
                {
                    Color = c,
                    Offset = ParseDoubleIgnoreNonDigits(element.Attribute("offset")?.Value),
                };
            }
            catch
            {
                gradientStop = null;
            }
            return false;
        }

        public static bool TryGetLinearGradientBrush(this XElement element, out LinearGradientBrush brush)
        {
            try
            {
                brush = new LinearGradientBrush()
                {
                    GradientStops = new GradientStopCollection(element.Elements().Where(c => "stop" == c.Name.LocalName && c.TryGetGradientStop(out GradientStop _)).Select(g => ()),
                    Transform = element.GetTransform("gradientTransorm"),
                };
                
            }
            catch
            {
                brush = null;
            }
            return false;
        }

        public static Transform GetTransform(this XElement element, string attributeName = "transform")
        {
            string transform = element.Attribute(attributeName)?.Value;
            if (null == transform) return Transform.Identity;

            try
            {
                string[] args = transform.Substring(transform.IndexOf('(') + 1).Trim(')').Split(new char[] { ' ', ',' });

                if(transform.StartsWith("matrix", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new MatrixTransform(double.Parse(args[0]), double.Parse(args[1]), double.Parse(args[2]), double.Parse(args[3]), double.Parse(args[4]), double.Parse(args[5]));
                }
                if (transform.StartsWith("translate", StringComparison.InvariantCultureIgnoreCase))
                {
                    double y = args.Length == 2 ? double.Parse(args[1]) : 0;
                    return new TranslateTransform(double.Parse(args[0]), 0);
                }
                if (transform.StartsWith("scale", StringComparison.InvariantCultureIgnoreCase))
                {
                    double y = args.Length == 2 ? double.Parse(args[1]) : double.Parse(args[0]);
                    return new ScaleTransform(double.Parse(args[0]), y);
                }
                if (transform.StartsWith("rotate", StringComparison.InvariantCultureIgnoreCase))
                {
                    double x = args.Length == 3 ? double.Parse(args[1]) : 0;
                    double y = args.Length == 3 ? double.Parse(args[2]) : 0;
                    return new RotateTransform(double.Parse(args[0]), x, y);
                }
                if (transform.StartsWith("skewx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SkewTransform(double.Parse(args[0]), 0);
                }
                if (transform.StartsWith("skewy", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SkewTransform(0, double.Parse(args[0]));
                }
            }
            catch
            {
                // ignore parse errors
            }
            return Transform.Identity;
        }

        public static Dictionary<string, object> GetDefinitions(this XElement element)
        {
            Dictionary<string, object> definitions = new Dictionary<string, object>();
            XElement defs = element.DescendantsAndSelf().Where(d => "defs" == d.Name.LocalName).FirstOrDefault();

            if(null != defs)
            {
                foreach(XElement definition in defs.Elements())
                {
                    string id = element.Attribute("id")?.Value;
                    if(null != id)
                    {
                        if(definition.TryGetShape(out Shape shape))
                        {
                            definitions[id] = shape;
                        }
                        else
                        {
                            switch (definition.Name.LocalName.ToLower())
                            {
                                case "lineargradient":
                                    if(definition.TryGetLinearGradientBrush(out LinearGradientBrush linearGradientBrush))
                                    {
                                        definitions[id] = linearGradientBrush;
                                    }
                                    break;
                                case "radialGradient":
                                    break;
                                case "pattern":
                                    break;
                            }
                        }
                    }
                }
            }

            return definitions;
        }
        #endregion

        #region Shape extensions
        public static void ApplyStyle(this Shape shape, Dictionary<string, string> style)
        {
            foreach (KeyValuePair<string, string> pair in style)
            {
                switch (pair.Key.ToLower())
                {
                    case "fill":
                        shape.Fill = ParseBrush(pair.Value);
                        break;
                    case "stroke":
                        shape.Stroke = ParseBrush(pair.Value);
                        break;
                    case "stroke-width":
                        shape.StrokeThickness = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-linecap":
                        shape.StrokeStartLineCap = ParseLineCap(pair.Value);
                        shape.StrokeEndLineCap = ParseLineCap(pair.Value);
                        break;
                    case "stroke-linejoin":
                        shape.StrokeLineJoin = ParseLineJoin(pair.Value);
                        break;
                    case "stroke-miterlimit":
                        shape.StrokeMiterLimit = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-dasharray":
                        shape.StrokeDashArray = ParseDashArray(pair.Value);
                        break;
                    case "stroke-dashoffset":
                        shape.StrokeDashOffset = ParseDoubleIgnoreNonDigits(pair.Value);
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
        #endregion

        #region Line extensions
        public static Point StartPoint(this Line line)
        {
            return new Point(line.X1, line.Y1);
        }

        public static Vector Vector(this Line line)
        {
            return new Vector(line.X2 - line.X1, line.Y2 - line.Y1);
        }
        #endregion

        #region PathFigure extensions
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
        #endregion
    }
}
