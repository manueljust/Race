using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Race
{
    public class Track
    {
        public Point StartPoint { get; set; } = new Point();
        public double StartAngle { get; set; } = 0;
        public double Width { get; set; } = 210;
        public double Height { get; set; } = 150;
        public Path Bounds { get; set; }
        public Path[] Obstacles { get; set; } = new Path[0];
        public Path[] Decorations { get; set; } = new Path[0];
        public Path Start { get; set; }
        public Path Goal { get; set; }
        public Brush Background { get; set; }


        public static Track FromSvg(string fileName)
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

            foreach(XElement element in xdoc.Descendants())
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
                        });
                        break;
                    case "ellipse":
                        path.Data = PathGeometry.CreateFromGeometry(new EllipseGeometry()
                        {
                            Center = new Point(element.GetDouble("cx"), element.GetDouble("cy")),
                            RadiusX = element.GetDouble("rx"),
                            RadiusY = element.GetDouble("ry"),
                        });
                        break;
                    case "line":
                        path.Data = PathGeometry.CreateFromGeometry(new LineGeometry()
                        {
                            StartPoint = new Point(element.GetDouble("x1"), element.GetDouble("y1")),
                            EndPoint = new Point(element.GetDouble("x2"), element.GetDouble("y2")),
                        });
                        break;
                    case "path":
                        // determine fillrule and transform
                        FillRule fillRule = style.ContainsKey("fill-rule") ? style["fill-rule"] == "nonzero" ? FillRule.Nonzero : FillRule.EvenOdd : FillRule.EvenOdd;
                        Transform transform = Transform.Identity;
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

                if(isPath)
                {
                    path.ApplyStyle(style);

                    switch (element.GetDescription()?.ToLower())
                    {
                        case "bounds":
                            track.Bounds = path;
                            break;
                        case "start":
                            track.Start = path;
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

            if(null == track.Goal)
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
    }
}
