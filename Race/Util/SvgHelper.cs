using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Race.Util
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
                            if (element.TryGetPath(out Path boundsPath, defs))
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
                            if (element.TryGetShape(out Shape obastcleShape, defs))
                            {
                                obstacles.Add(obastcleShape);
                            }
                            break;
                        default:
                            if (element.TryGetShape(out Shape decorationShape, defs))
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
    }
}
