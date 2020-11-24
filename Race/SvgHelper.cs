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

            List<Path> decorations = new List<Path>();
            List<Path> obstacles = new List<Path>();

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

            foreach (XElement element in xdoc.Descendants())
            {
                Path path = new Path()
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
                        // determine fillrule and transform
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
                    //path.GeometryTransform = element.GetTransform();

                    switch (element.GetDescription()?.ToLower())
                    {
                        case "bounds":
                            track.Bounds = path;
                            break;
                        case "start":
                            track.Start = path;
                            track.StartPoint = path.Data.Bounds.Location;
                            track.StartLine = new Vector(path.Data.Bounds.Width, path.Data.Bounds.Height);
                            // get outline, determine center
                            break;
                        case "goal":
                            track.Goal = path;
                            break;
                        case "obstacle":
                            obstacles.Add(path);
                            break;
                        default:
                            decorations.Add(path);
                            break;
                    }
                }
            }

            track.Obstacles = obstacles.ToArray();
            track.Decorations = decorations.ToArray();

            if (null == track.Goal)
            {
                track.Goal = (Path)System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(track.Start));
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
            Dictionary<string, string> style = element.GetStyle();

            FormattedText text = new FormattedText(
                element.Value,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily(style["font-family"]),
                    ParseFontStyle(style["font-style"]),
                    ParseFontWeight(style["font-weight"]),
                    FontStretches.Normal),
                ParseDoubleIgnoreNonDigits(style["font-size"]),
                ParseBrush(style["fill"])
                );

            Geometry geometry = text.BuildGeometry(new Point(element.GetDouble("x"), element.GetDouble("y")));
            geometry.Transform = element.GetTransform();

            return geometry;

        }

        public static string GetDescription(this XElement element)
        {
            foreach(XElement child in element.Elements())
            {
                if(child.Name.LocalName == "desc")
                {
                    return child.Value;
                }
            }
            return null;
        }

        public static Dictionary<string, string> GetStyle(this XElement element)
        {
            Dictionary<string, string> style = new Dictionary<string, string>();

            XAttribute st = element.Attribute("style");
            if(null != st)
            {
                string[] styles = st.Value.Split(';');
                foreach(string s in styles)
                {
                    string[] kv = s.Split(':');
                    if(2 == kv.Length)
                    {
                        style[kv[0]] = kv[1];
                    }
                }
            }

            return style;
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


        public static void ApplyStyle(this Path path, Dictionary<string, string> style)
        {
            foreach (KeyValuePair<string, string> pair in style)
            {
                switch (pair.Key.ToLower())
                {
                    case "fill":
                        path.Fill = ParseBrush(pair.Value);
                        break;
                    case "stroke":
                        path.Stroke = ParseBrush(pair.Value);
                        break;
                    case "stroke-width":
                        path.StrokeThickness = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-linecap":
                        path.StrokeStartLineCap = ParseLineCap(pair.Value);
                        path.StrokeEndLineCap = ParseLineCap(pair.Value);
                        break;
                    case "stroke-linejoin":
                        path.StrokeLineJoin = ParseLineJoin(pair.Value);
                        break;
                    case "stroke-miterlimit":
                        path.StrokeMiterLimit = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-dasharray":
                        path.StrokeDashArray = ParseDashArray(pair.Value);
                        break;
                    case "stroke-dashoffset":
                        path.StrokeDashOffset = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "visibility":
                        switch (pair.Value)
                        {
                            case "hidden":
                                path.Visibility = Visibility.Hidden;
                                break;
                            case "collapsed":
                                path.Visibility = Visibility.Collapsed;
                                break;
                        }
                        break;
                    case "opacity":
                        path.Opacity = double.Parse(pair.Value);
                        break;
                }
            }
        }
    }
}
