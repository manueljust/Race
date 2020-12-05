using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Race.Util
{
    public static class XElementExtensions
    {
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

        public static Brush GetBrush(this XElement element, string name, Dictionary<string, object> definitions = null)
        {
            XAttribute att = element.Attribute(name);
            if (null != att)
            {
                return Parser.ParseBrush(att.Value, definitions);
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
                foreach (Geometry spanGeometry in spanGeometries)
                {
                    baseGeometry = new CombinedGeometry(baseGeometry, spanGeometry);
                }
                baseGeometry.Transform = element.GetTransform();
                return baseGeometry;
            }
        }

        public static Geometry GetTextSpanGeometry(this XElement element, Dictionary<string, string> baseStyle)
        {
            Dictionary<string, string> style = element.GetStyle(baseStyle);

            double fontSize = Parser.ParseDoubleIgnoreNonDigits(style["font-size"]);

            FormattedText text = new FormattedText(
                element.Value,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily(style["font-family"]),
                    Parser.ParseFontStyle(style["font-style"]),
                    Parser.ParseFontWeight(style["font-weight"]),
                    FontStretches.Normal),
                fontSize,
                Parser.ParseBrush(style["fill"])
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
            if (null != styleElement)
            {
                string[] styleStrings = styleElement.Value.Split(';');
                foreach (string styleString in styleStrings)
                {
                    string[] keyValueStrings = styleString.Split(':');
                    if (2 == keyValueStrings.Length)
                    {
                        style[keyValueStrings[0]] = keyValueStrings[1];
                    }
                }
            }

            return style;
        }

        public static bool TryGetShape(this XElement element, out Shape shape, Dictionary<string, object> definitions = null)
        {
            switch (element.Name.LocalName.ToLower())
            {
                case "circle":
                    if (element.TryGetCircle(out Ellipse circle, definitions))
                    {
                        shape = circle;
                        return true;
                    }
                    break;
                case "ellipse":
                    if (element.TryGetEllipse(out Ellipse ellipse, definitions))
                    {
                        shape = ellipse;
                        return true;
                    }
                    break;
                case "line":
                    if (element.TryGetLine(out Line line, definitions))
                    {
                        shape = line;
                        return true;
                    }
                    break;
                case "path":
                    if (element.TryGetPath(out Path path, definitions))
                    {
                        shape = path;
                        return true;
                    }
                    break;
                case "polygon":
                    if (element.TryGetPolygon(out Polygon polygon, definitions))
                    {
                        shape = polygon;
                        return true;
                    }
                    break;
                case "polyline":
                    if (element.TryGetPolyline(out Polyline polyline, definitions))
                    {
                        shape = polyline;
                        return true;
                    }
                    break;
                case "rect":
                    if (element.TryGetRectangle(out Rectangle rectangle, definitions))
                    {
                        shape = rectangle;
                        return true;
                    }
                    break;
                case "text":
                    if (element.TryGetText(out Path text, definitions))
                    {
                        shape = text;
                        return true;
                    }
                    break;
                case "image":
                    if(element.TryGetImageRectangle(out Rectangle imageRectangle, definitions))
                    {
                        shape = imageRectangle;
                        return true;
                    }
                    break;
            }
            shape = null;
            return false;
        }

        public static bool TryGetCircle(this XElement element, out Ellipse ellipse, Dictionary<string, object> definitions = null)
        {
            try
            {
                double x = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("cx")?.Value);
                double y = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("cy")?.Value);
                double r = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("r")?.Value);

                ellipse = new Ellipse()
                {
                    Fill = element.GetBrush("fill", definitions),
                    Stroke = element.GetBrush("stroke"),
                    Height = 2 * r,
                    Width = 2 * r,
                    RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { element.GetTransform(), new TranslateTransform(x - r, y - r) }) },
                };

                ellipse.ApplyStyle(element.GetStyle(), definitions);

                return true;
            }
            catch
            {
                ellipse = null;
            }
            return false;
        }

        public static bool TryGetEllipse(this XElement element, out Ellipse ellipse, Dictionary<string, object> definitions = null)
        {
            try
            {
                double x = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("cx")?.Value);
                double y = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("cy")?.Value);
                double rx = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("rx")?.Value);
                double ry = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("ry")?.Value);

                ellipse = new Ellipse()
                {
                    Fill = element.GetBrush("fill", definitions),
                    Stroke = element.GetBrush("stroke"),
                    Height = 2 * ry,
                    Width = 2 * rx,
                    RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { element.GetTransform(), new TranslateTransform(x - rx, y - ry) }) },
                };

                ellipse.ApplyStyle(element.GetStyle(), definitions);

                return true;
            }
            catch
            {
                ellipse = null;
            }
            return false;
        }

        public static bool TryGetLine(this XElement element, out Line line, Dictionary<string, object> definitions = null)
        {
            Dictionary<string, string> style = element.GetStyle();

            try
            {
                line = new Line()
                {
                    Fill = element.GetBrush("fill", definitions),
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

        public static bool TryGetPath(this XElement element, out Path path, Dictionary<string, object> definitions = null)
        {
            try
            {
                path = new Path()
                {
                    Fill = element.GetBrush("fill", definitions),
                    Stroke = element.GetBrush("stroke"),
                };

                Dictionary<string, string> style = element.GetStyle();
                FillRule fillRule = style.ContainsKey("fill-rule") ? style["fill-rule"] == "nonzero" ? FillRule.Nonzero : FillRule.EvenOdd : FillRule.EvenOdd;
                Transform transform = element.GetTransform();
                path.Data = new PathGeometry((PathFigureCollection)(new PathFigureCollectionConverter()).ConvertFromString(element.Attribute("d").Value), fillRule, transform);

                path.ApplyStyle(style, definitions);
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        public static bool TryGetPolygon(this XElement element, out Polygon polygon, Dictionary<string, object> definitions = null)
        {
            try
            {
                polygon = new Polygon()
                {
                    Fill = element.GetBrush("fill", definitions),
                    Stroke = element.GetBrush("stroke"),
                    Points = new PointCollection(element.Attribute("points").Value.Split(' ').Select(s => Point.Parse(s))),
                    RenderTransform = element.GetTransform(),
                };

                polygon.ApplyStyle(element.GetStyle(), definitions);
                return true;
            }
            catch
            {
                polygon = null;
                return false;
            }
        }

        public static bool TryGetPolyline(this XElement element, out Polyline polyline, Dictionary<string, object> definitions = null)
        {
            try
            {
                polyline = new Polyline()
                {
                    Fill = element.GetBrush("fill", definitions),
                    Stroke = element.GetBrush("stroke"),
                    Points = new PointCollection(element.Attribute("points").Value.Split(' ').Select(s => Point.Parse(s))),
                    RenderTransform = element.GetTransform(),
                };

                polyline.ApplyStyle(element.GetStyle(), definitions);
                return true;
            }
            catch
            {
                polyline = null;
                return false;
            }
        }

        public static bool TryGetRectangle(this XElement element, out Rectangle rectangle, Dictionary<string, object> definitions = null)
        {
            try
            {
                double x = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("x")?.Value);
                double y = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("y")?.Value);
                double w = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("width")?.Value);
                double h = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("height")?.Value);
                double rx = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("rx")?.Value);
                double ry = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("ry")?.Value);

                rectangle = new Rectangle()
                {
                    Fill = element.GetBrush("fill", definitions),
                    Stroke = element.GetBrush("stroke"),
                    Width = w,
                    Height = h,
                    RadiusX = 0.0 == rx ? ry : rx,
                    RadiusY = 0.0 == ry ? rx : ry,
                    RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { element.GetTransform(), new TranslateTransform(x, y) }) },
                };

                rectangle.ApplyStyle(element.GetStyle(), definitions);
                return true;
            }
            catch
            {
                rectangle = null;
                return false;
            }
        }

        public static bool TryGetImageRectangle(this XElement element, out Rectangle imageRectangle, Dictionary<string, object> definitions = null)
        {
            try
            {
                double width = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("width")?.Value);
                double height = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("height")?.Value);
                double x = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("x")?.Value);
                double y = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("y")?.Value);
                string aspectRatio = element.Attribute("preserveAspectRatio")?.Value ?? "none";

                imageRectangle = new Rectangle()
                {
                    Fill = element.GetImageBrush(definitions),
                    Width = width,
                    Height = height,
                    RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { new TranslateTransform(x, y), element.GetTransform() }) },
                };

                imageRectangle.ApplyStyle(element.GetStyle(new Dictionary<string, string>()), definitions);
                return true;
            }
            catch
            {
                imageRectangle = null;
                return false;
            }
        }

        public static ImageBrush GetImageBrush(this XElement element, Dictionary<string, object> definitions = null)
        {
            string link = element.Attributes().Where(a => a.Name.LocalName == "href").FirstOrDefault()?.Value ?? "";
            return new ImageBrush()
            {
                ImageSource = link.GetImageSourceFromSvgLink(),
            };
        }


        public static bool TryGetText(this XElement element, out Path text, Dictionary<string, object> definitions = null)
        {
            try
            {
                double x = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("x")?.Value);
                double y = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("y")?.Value);

                text = new Path()
                {
                    Data = PathGeometry.CreateFromGeometry(element.GetTextGeometry()),
                };

                text.ApplyStyle(element.GetStyle(), definitions);
                return true;
            }
            catch
            {
                text = null;
                return false;
            }
        }

        public static IEnumerable<GradientStop> GetGradientStops(this XElement element)
        {
            List<GradientStop> stops = new List<GradientStop>();
            foreach (XElement stop in element.Elements().Where(c => "stop" == c.Name.LocalName))
            {
                if (stop.TryGetGradientStop(out GradientStop gradientStop))
                {
                    stops.Add(gradientStop);
                }
            }
            return stops;
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
                    Offset = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("offset")?.Value),
                };
                return true;
            }
            catch
            {
                gradientStop = null;
            }
            return false;
        }

        public static bool TryGetLinearGradientBrush(this XElement element, out LinearGradientBrush brush, Dictionary<string, object> definitions = null)
        {
            try
            {
                double x1 = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("x1")?.Value);
                double y1 = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("y1")?.Value);
                double x2 = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("x2")?.Value);
                double y2 = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("y2")?.Value);

                GradientStopCollection stops;
                string linkId = element.Attributes().Where(a => a.Name.LocalName == "href").FirstOrDefault()?.Value.Substring(1) ?? "";
                if (null != definitions && definitions.ContainsKey(linkId))
                {
                    stops = ((LinearGradientBrush)definitions[linkId]).GradientStops;
                }
                else
                {
                    stops = new GradientStopCollection(element.GetGradientStops());
                }

                brush = new LinearGradientBrush()
                {
                    MappingMode = BrushMappingMode.Absolute,
                    GradientStops = stops,
                    StartPoint = new Point(x1, y1),
                    EndPoint = new Point(x2, y2),
                    Transform = element.GetTransform("gradientTransorm"),
                };

                return true;
            }
            catch
            {
                brush = null;
            }
            return false;
        }

        public static bool TryGetRadialGradientBrush(this XElement element, out RadialGradientBrush brush, Dictionary<string, object> definitions = null)
        {
            try
            {
                double x = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("cx")?.Value);
                double y = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("cy")?.Value);
                double r = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("r")?.Value);
                double fr = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("fr")?.Value);
                double fx = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("fx")?.Value);
                double fy = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("fy")?.Value);
                GradientSpreadMethod spreadMethod = Parser.ParseGradiendSpreadMethod(element.Attribute("spreadMethod")?.Value);


                GradientStopCollection stops;
                string linkId = element.Attributes().Where(a => a.Name.LocalName == "href").FirstOrDefault()?.Value.Substring(1) ?? "";
                if (null != definitions && definitions.ContainsKey(linkId))
                {
                    stops = ((LinearGradientBrush)definitions[linkId]).GradientStops;
                }
                else
                {
                    stops = new GradientStopCollection(element.GetGradientStops());
                }

                brush = new RadialGradientBrush()
                {
                    MappingMode = BrushMappingMode.Absolute,
                    Center = new Point(x, y),
                    RadiusX = r,
                    RadiusY = r,
                    GradientOrigin = new Point(fx, fy),
                    GradientStops = stops,
                    SpreadMethod = spreadMethod,
                    Transform = element.GetTransform("gradientTransorm"),
                };

                return true;
            }
            catch
            {
                brush = null;
            }
            return false;
        }

        public static bool TryGetVisualBrush(this XElement element, out VisualBrush brush, Dictionary<string, object> definitions = null)
        {
            try
            {
                string linkId = element.Attributes().Where(a => a.Name.LocalName == "href").FirstOrDefault()?.Value.Substring(1) ?? "";
                if (null != definitions && definitions.ContainsKey(linkId))
                {
                    brush = ((VisualBrush)definitions[linkId]).Clone();
                    brush.Transform = element.GetTransform();
                    return true;
                }


                double width = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("width")?.Value);
                double height = Parser.ParseDoubleIgnoreNonDigits(element.Attribute("height")?.Value);

                System.Windows.Controls.Canvas c = new System.Windows.Controls.Canvas()
                {
                    Height = height,
                    Width = width,
                };
                foreach(XElement child in element.Elements())
                {
                    if(child.TryGetShape(out Shape shape, definitions))
                    {
                        c.Children.Add(shape);
                    }
                }

                brush = new VisualBrush()
                {
                    Viewbox = new Rect(new Point(0, 0), new Size(width, height)),
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewport = new Rect(new Point(0, 0), new Size(width, height)),
                    ViewportUnits = BrushMappingMode.Absolute,
                    TileMode = TileMode.Tile,
                    Visual = c,
                };
                if(brush.CanFreeze) brush.Freeze();

                return true;
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

                if (transform.StartsWith("matrix", StringComparison.InvariantCultureIgnoreCase))
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
                    double x = Parser.ParseDoubleIgnoreNonDigits(element.Attributes().FirstOrDefault(a => "transform-center-x" == a.Name.LocalName)?.Value);
                    double y = Parser.ParseDoubleIgnoreNonDigits(element.Attributes().FirstOrDefault(a => "transform-center-y" == a.Name.LocalName)?.Value);
                    if(3 == args.Length)
                    {
                        x = double.Parse(args[1]);
                        y = double.Parse(args[2]);
                    }
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
            XElement defs = element.Descendants().Where(d => "defs" == d.Name.LocalName).FirstOrDefault();

            if (null != defs)
            {
                foreach (XElement definition in defs.Elements())
                {
                    string id = definition.Attribute("id")?.Value;
                    if (null != id)
                    {
                        if (definition.TryGetShape(out Shape shape, definitions))
                        {
                            definitions[id] = shape;
                        }
                        else
                        {
                            switch (definition.Name.LocalName.ToLower())
                            {
                                case "lineargradient":
                                    if (definition.TryGetLinearGradientBrush(out LinearGradientBrush linearGradientBrush, definitions))
                                    {
                                        definitions[id] = linearGradientBrush;
                                    }
                                    break;
                                case "radialgradient":
                                    if(definition.TryGetRadialGradientBrush(out RadialGradientBrush radialGradientBrush, definitions))
                                    {
                                        definitions[id] = radialGradientBrush;
                                    }
                                    break;
                                case "pattern":
                                    if(definition.TryGetVisualBrush(out VisualBrush visualBrush, definitions))
                                    {
                                        definitions[id] = visualBrush;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            foreach(Freezable f in definitions.Values.OfType<Freezable>())
            {
                if(f.CanFreeze)
                {
                    f.Freeze();
                }
            }
            return definitions;
        }
    }
}
