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
                StartPoint = new Point(65, 40),
                StartAngle = 0.22,
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

            List<Path> decorations = new List<Path>();
            List<Path> obstacles = new List<Path>();
            foreach (XElement element in xdoc.Descendants())
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
                                track.StartPoint = new Point(startLine.X1, startLine.X2);
                                track.StartLine = new Vector(startLine.X2 - startLine.X1, startLine.Y2 - startLine.Y2);
                            }
                            break;
                        case "goal":
                            if (element.TryGetLine(out Line goalLine))
                            {
                                track.Goal = goalLine;
                            }
                            break;
                        case "obstacle":
                            if (element.TryGetPath(out Path obastclePath))
                            {
                                obstacles.Add(obastclePath);
                            }
                            break;
                        default:
                            if (element.TryGetPath(out Path decorationPath))
                            {
                                decorations.Add(decorationPath);
                            }
                            break;
                    }
            }
            track.Obstacles = obstacles.ToArray();
            track.Decorations = decorations.ToArray();

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
            if (null == track.Start)
            {
                throw new InvalidOperationException("No element with description \"start\" found.");
            }

            return track;
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

        public static bool TryGetPath(this XElement element, out Path path)
        {
            path = new Path()
            {
                Fill = element.GetBrush("fill"),
                Stroke = element.GetBrush("stroke"),
            };

            bool isPath = true;

            Dictionary<string, string> style = element.GetStyle();

            switch (element.Name.LocalName.ToLower())
            {
                case "circle":
                    path.Data = PathGeometry.CreateFromGeometry(new EllipseGeometry()
                    {
                        Center = new Point(element.GetDouble("cx"), element.GetDouble("cy")),
                        RadiusX = element.GetDouble("r"),
                        RadiusY = element.GetDouble("r"),
                        Transform = element.GetTransform(),
                    });
                    break;
                case "ellipse":
                    path.Data = PathGeometry.CreateFromGeometry(new EllipseGeometry()
                    {
                        Center = new Point(element.GetDouble("cx"), element.GetDouble("cy")),
                        RadiusX = element.GetDouble("rx"),
                        RadiusY = element.GetDouble("ry"),
                        Transform = element.GetTransform(),
                    });
                    break;
                case "line":
                    path.Data = PathGeometry.CreateFromGeometry(new LineGeometry()
                    {
                        StartPoint = new Point(element.GetDouble("x1"), element.GetDouble("y1")),
                        EndPoint = new Point(element.GetDouble("x2"), element.GetDouble("y2")),
                        Transform = element.GetTransform(),
                    });
                    break;
                case "path":
                    FillRule fillRule = style.ContainsKey("fill-rule") ? style["fill-rule"] == "nonzero" ? FillRule.Nonzero : FillRule.EvenOdd : FillRule.EvenOdd;
                    Transform transform = element.GetTransform();
                    path.Data = new PathGeometry((PathFigureCollection)(new PathFigureCollectionConverter()).ConvertFromString(element.Attribute("d").Value), fillRule, transform);
                    break;
                case "polygon":
                    path.Data = PathGeometry.CreateFromGeometry((new Polygon()
                    {
                        Points = new PointCollection(element.Attribute("points").Value.Split(' ').Select(s => Point.Parse(s))),
                    }).RenderedGeometry);
                    break;
                case "polyline":
                    path.Data = PathGeometry.CreateFromGeometry((new Polyline()
                    {
                        Points = new PointCollection(element.Attribute("points").Value.Split(' ').Select(s => Point.Parse(s))),
                    }).RenderedGeometry);
                    break;
                case "rect":
                    path.Data = PathGeometry.CreateFromGeometry(new RectangleGeometry()
                    {
                        Rect = new Rect(new Point(element.GetDouble("x"), element.GetDouble("y")), new Size(element.GetDouble("width"), element.GetDouble("height"))),
                        RadiusX = element.GetDouble("rx"),
                        RadiusY = element.GetDouble("ry", element.GetDouble("rx")),
                        Transform = element.GetTransform(),
                    });
                    break;
                case "text":
                    path.Data = PathGeometry.CreateFromGeometry(element.GetTextGeometry());
                    break;
                case "image":
                default:
                    isPath = false;
                    break;
            }

            if (isPath)
            {
                path.ApplyStyle(style);
                return true;
            }
            return false;
        }

        public static bool TryGetLine(this XElement element, out Line line)
        {
            line = new Line()
            {
                Fill = element.GetBrush("fill"),
                Stroke = element.GetBrush("stroke"),
            };

            bool isPath = true;

            Dictionary<string, string> style = element.GetStyle();

            try
            {
                switch (element.Name.LocalName.ToLower())
                {
                    case "line":
                        line.X1 = element.GetDouble("x1");
                        line.Y1 = element.GetDouble("y1");
                        line.X2 = element.GetDouble("x2");
                        line.Y2 = element.GetDouble("y2");
                        break;
                    case "path":
                        FillRule fillRule = FillRule.EvenOdd;
                        Transform transform = element.GetTransform();
                        PathGeometry pg = new PathGeometry((PathFigureCollection)(new PathFigureCollectionConverter()).ConvertFromString(element.Attribute("d").Value), fillRule, transform);
                        line.X1 = pg.Figures.First().StartPoint.X;
                        line.Y1 = pg.Figures.First().StartPoint.Y;
                        line.X2 = pg.Figures.First().EndPoint().X;
                        line.Y2 = pg.Figures.First().EndPoint().Y;
                        break;
                    default:
                        isPath = false;
                        break;
                }
            }
            catch
            {
                return false;
            }

            if (isPath)
            {
                line.ApplyStyle(style);
                return true;
            }
            return false;
        }

        public static Transform GetTransform(this XElement element)
        {
            string transform = element.Attribute("transform")?.Value;
            if (null == transform) return Transform.Identity;

            try
            {
                string[] v = transform.Substring(transform.IndexOf('(') + 1).Trim(')').Split(new char[] { ' ', ',' });

                if(transform.StartsWith("matrix", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new MatrixTransform(double.Parse(v[0]), double.Parse(v[1]), double.Parse(v[2]), double.Parse(v[3]), double.Parse(v[4]), double.Parse(v[5]));
                }
                if (transform.StartsWith("translate", StringComparison.InvariantCultureIgnoreCase))
                {
                    double y = v.Length == 2 ? double.Parse(v[1]) : 0;
                    return new TranslateTransform(double.Parse(v[0]), 0);
                }
                if (transform.StartsWith("scale", StringComparison.InvariantCultureIgnoreCase))
                {
                    double y = v.Length == 2 ? double.Parse(v[1]) : double.Parse(v[0]);
                    return new ScaleTransform(double.Parse(v[0]), y);
                }
                if (transform.StartsWith("rotate", StringComparison.InvariantCultureIgnoreCase))
                {
                    double x = v.Length == 3 ? double.Parse(v[1]) : 0;
                    double y = v.Length == 3 ? double.Parse(v[2]) : 0;
                    return new RotateTransform(double.Parse(v[0]), x, y);
                }
                if (transform.StartsWith("skewx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SkewTransform(double.Parse(v[0]), 0);
                }
                if (transform.StartsWith("skewy", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SkewTransform(0, double.Parse(v[0]));
                }
            }
            catch
            {
                // ignore parse errors
            }
            return Transform.Identity;
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
        #endregion

        #region Line extensions
        public static Point StartPoint(this Line line)
        {
            //        d="m 139.57621,16.301715 2.29859,15.021964"

            return new Point(line.X1 + line.GeometryTransform.Value.OffsetX, line.Y1 + line.GeometryTransform.Value.OffsetY);
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
