using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Race
{
    public class Player : PropertyChangedAware
    {
        private string _name = "Name";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private Color _color = Colors.Red;
        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        private double _ratio = 0.6;
        public double Ratio
        {
            get { return _ratio; }
            set { SetProperty(ref _ratio, Constrained(value, 0.1, 0.9)); }
        }

        private double _handicap = 1;
        public double Handicap
        {
            get { return _handicap; }
            set { SetProperty(ref _handicap, Constrained(value, 0.1, 1)); }
        }

        private static double Constrained(double value, double min, double max)
        {
            return double.NaN == value ? value : min > value ? min : max < value ? max : value;
        }


        public static Color[] PredefinedColors { get; } = new Color[]
        {
            Colors.Red,
            Colors.Teal,
            Colors.Blue,
            Colors.Orange,
            Colors.Olive,
            Colors.Chartreuse,
        };
    }
}
