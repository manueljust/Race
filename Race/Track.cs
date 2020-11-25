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
        public Vector StartLine { get; set; } = new Vector();
        public double StartAngle { get; set; } = 0;
        public double Width { get; set; } = 210;
        public double Height { get; set; } = 150;
        public Path Bounds { get; set; }
        public Path[] Obstacles { get; set; } = new Path[0];
        public Path[] Decorations { get; set; } = new Path[0];
        public Line Start { get; set; }
        public Line Goal { get; set; }
        public Brush Background { get; set; }


        public static Track FromSvg(string fileName)
        {
            return SvgHelper.Load(fileName);
        }
    }
}
