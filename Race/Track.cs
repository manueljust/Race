using System.Windows.Media;
using System.Windows.Shapes;

namespace Race
{
    public class Track
    {
        public double Width { get; set; } = 210;
        public double Height { get; set; } = 150;
        public Path Bounds { get; set; }
        public Path[] Obstacles { get; set; } = new Path[0];
        public Path[] Decorations { get; set; } = new Path[0];
        public Line Start { get; set; }
        public Line Goal { get; set; }
        public Brush Background { get; set; }
    }
}
